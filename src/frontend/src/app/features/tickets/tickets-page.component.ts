import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroupDirective, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { finalize, forkJoin } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { Project } from '../projects/project.models';
import { ProjectsService } from '../projects/projects.service';
import { User } from '../users/user.models';
import { UsersService } from '../users/users.service';
import {
  CreateTicketRequest,
  getAssignedDeveloperNames as formatAssignedDeveloperNames,
  getTicketPriorityLabel,
  getTicketStatusLabel,
  Ticket,
  TicketPriority,
  ticketPriorityOptions,
  TicketStatus,
  ticketStatusOptions,
  UpdateTicketRequest
} from './ticket.models';
import { TicketsService } from './tickets.service';

const ticketTitleMaxLength = 200;
const ticketDescriptionMaxLength = 4000;

function requiredTrimmedValidator(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length > 0 ? null : { requiredTrimmed: true };
}

@Component({
  selector: 'itm-tickets-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule
  ],
  templateUrl: './tickets-page.component.html',
  styleUrls: ['./tickets-page.component.css']
})
export class TicketsPageComponent implements OnInit {
  @ViewChild(FormGroupDirective) private ticketFormDirective?: FormGroupDirective;

  readonly ticketTitleMaxLength = ticketTitleMaxLength;
  readonly ticketDescriptionMaxLength = ticketDescriptionMaxLength;
  readonly statusOptions = ticketStatusOptions;
  readonly priorityOptions = ticketPriorityOptions;

  readonly ticketForm = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(ticketTitleMaxLength), requiredTrimmedValidator]],
    description: ['', [Validators.maxLength(ticketDescriptionMaxLength)]],
    projectId: ['', [Validators.required]],
    status: [1 as TicketStatus, [Validators.required]],
    priority: [2 as TicketPriority, [Validators.required]],
    assignedDeveloperIds: [[] as string[]]
  });

  tickets: Ticket[] = [];
  projects: Project[] = [];
  developers: User[] = [];
  isLoading = false;
  isSubmitting = false;
  deletingTicketId: string | null = null;
  loadErrorMessage = '';
  submitErrorMessage = '';
  editingTicketId: string | null = null;

  constructor(
    private readonly authService: AuthService,
    private readonly formBuilder: FormBuilder,
    private readonly projectsService: ProjectsService,
    private readonly ticketsService: TicketsService,
    private readonly usersService: UsersService
  ) {}

  get canManageTickets(): boolean {
    return this.authService.hasRole('Admin') || this.authService.hasRole('Developer');
  }

  get isAdmin(): boolean {
    return this.authService.hasRole('Admin');
  }

  get isEditMode(): boolean {
    return this.editingTicketId !== null;
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.loadErrorMessage = '';

    forkJoin({
      projects: this.projectsService.getProjects(),
      tickets: this.ticketsService.getTickets(),
      developers: this.usersService.getDevelopers()
    })
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: ({ projects, tickets, developers }) => {
          this.projects = sortProjectsByName(projects);
          this.tickets = sortTicketsByUpdatedAt(tickets);
          this.developers = developers;
        },
        error: () => {
          this.loadErrorMessage = 'Tickets could not be loaded right now. Try again.';
        }
      });
  }

  startEdit(ticket: Ticket): void {
    if (!this.canManageTickets) {
      return;
    }

    this.editingTicketId = ticket.id;
    this.submitErrorMessage = '';
    this.ticketForm.setValue({
      title: ticket.title,
      description: ticket.description ?? '',
      projectId: ticket.projectId,
      status: ticket.status,
      priority: ticket.priority,
      assignedDeveloperIds: ticket.assignedDevelopers.map((developer) => developer.id)
    });
  }

  cancelEdit(): void {
    if (!this.canManageTickets) {
      return;
    }

    this.resetForm();
  }

  submit(): void {
    if (!this.canManageTickets) {
      return;
    }

    if (this.ticketForm.invalid) {
      this.ticketForm.markAllAsTouched();
      return;
    }

    const session = this.authService.getSession();
    if (!session) {
      this.submitErrorMessage = 'Your session expired. Sign in again.';
      return;
    }

    this.isSubmitting = true;
    this.submitErrorMessage = '';

    const submitOperation = this.editingTicketId
      ? this.ticketsService.updateTicket(this.editingTicketId, this.toUpdateTicketRequest())
      : this.ticketsService.createTicket(this.toCreateTicketRequest(session.username));

    submitOperation.pipe(finalize(() => (this.isSubmitting = false))).subscribe({
      next: (ticket) => {
        this.tickets = upsertTicket(this.tickets, ticket);
        this.resetForm();
      },
      error: () => {
        this.submitErrorMessage = this.editingTicketId
          ? 'The ticket could not be updated. Review the form and try again.'
          : 'The ticket could not be created. Review the form and try again.';
      }
    });
  }

  deleteTicket(ticket: Ticket): void {
    if (!this.isAdmin || this.deletingTicketId) {
      return;
    }

    const confirmed = window.confirm(`Delete ticket "${ticket.title}"?`);
    if (!confirmed) {
      return;
    }

    this.deletingTicketId = ticket.id;
    this.submitErrorMessage = '';

    this.ticketsService
      .deleteTicket(ticket.id)
      .pipe(finalize(() => (this.deletingTicketId = null)))
      .subscribe({
        next: () => {
          this.tickets = this.tickets.filter((existingTicket) => existingTicket.id !== ticket.id);

          if (this.editingTicketId === ticket.id) {
            this.resetForm();
          }
        },
        error: () => {
          this.submitErrorMessage = 'The ticket could not be deleted right now. Try again.';
        }
      });
  }

  getProjectName(projectId: string): string {
    return this.projects.find((project) => project.id === projectId)?.name ?? 'Unknown project';
  }

  getStatusLabel(status: TicketStatus): string {
    return getTicketStatusLabel(status);
  }

  getPriorityLabel(priority: TicketPriority): string {
    return getTicketPriorityLabel(priority);
  }

  getAssignedDeveloperNames(ticket: Ticket): string {
    return formatAssignedDeveloperNames(ticket.assignedDevelopers);
  }

  trackByTicketId(_: number, ticket: Ticket): string {
    return ticket.id;
  }

  trackByDeveloperId(_: number, developer: User): string {
    return developer.id;
  }

  private resetForm(): void {
    this.editingTicketId = null;
    this.submitErrorMessage = '';
    this.ticketFormDirective?.resetForm({
      title: '',
      description: '',
      projectId: '',
      status: 1,
      priority: 2,
      assignedDeveloperIds: []
    });
    this.ticketForm.reset({
      title: '',
      description: '',
      projectId: '',
      status: 1,
      priority: 2,
      assignedDeveloperIds: []
    });
    this.ticketForm.markAsPristine();
    this.ticketForm.markAsUntouched();
    this.ticketForm.updateValueAndValidity({ emitEvent: false });
  }

  private toCreateTicketRequest(createdByUsername: string): CreateTicketRequest {
    return {
      ...this.toBaseTicketRequest(),
      createdByUsername: createdByUsername.trim()
    };
  }

  private toUpdateTicketRequest(): UpdateTicketRequest {
    return this.toBaseTicketRequest();
  }

  private toBaseTicketRequest(): Omit<CreateTicketRequest, 'createdByUsername'> {
    const formValue = this.ticketForm.getRawValue();

    return {
      title: formValue.title.trim(),
      description: normalizeOptionalValue(formValue.description),
      projectId: formValue.projectId,
      status: formValue.status,
      priority: formValue.priority,
      assignedDeveloperIds: [...new Set(formValue.assignedDeveloperIds)]
    };
  }
}

function normalizeOptionalValue(value: string): string | null {
  const trimmedValue = value.trim();
  return trimmedValue.length > 0 ? trimmedValue : null;
}

function sortProjectsByName(projects: Project[]): Project[] {
  return [...projects].sort((left, right) => left.name.localeCompare(right.name));
}

function sortTicketsByUpdatedAt(tickets: Ticket[]): Ticket[] {
  return [...tickets].sort((left, right) => new Date(right.updatedAtUtc).getTime() - new Date(left.updatedAtUtc).getTime());
}

function upsertTicket(tickets: Ticket[], savedTicket: Ticket): Ticket[] {
  const remainingTickets = tickets.filter((ticket) => ticket.id !== savedTicket.id);
  return sortTicketsByUpdatedAt([...remainingTickets, savedTicket]);
}
