import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroupDirective, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { PageEvent, MatPaginatorModule } from '@angular/material/paginator';
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
  PagedResult,
  getAssignedDeveloperNames as formatAssignedDeveloperNames,
  getTicketPriorityLabel,
  getTicketStatusLabel,
  Ticket,
  TicketQuery,
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
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatSelectModule
  ],
  templateUrl: './tickets-page.component.html',
  styleUrls: ['./tickets-page.component.css']
})
export class TicketsPageComponent implements OnInit {
  @ViewChild(FormGroupDirective) private ticketFormDirective?: FormGroupDirective;

  private readonly defaultPageSize = 10;

  readonly ticketTitleMaxLength = ticketTitleMaxLength;
  readonly ticketDescriptionMaxLength = ticketDescriptionMaxLength;
  readonly statusOptions = ticketStatusOptions;
  readonly priorityOptions = ticketPriorityOptions;
  readonly pageSizeOptions = [5, 10, 20];

  readonly ticketForm = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(ticketTitleMaxLength), requiredTrimmedValidator]],
    description: ['', [Validators.maxLength(ticketDescriptionMaxLength)]],
    projectId: ['', [Validators.required]],
    status: [1 as TicketStatus, [Validators.required]],
    priority: [2 as TicketPriority, [Validators.required]],
    assignedDeveloperIds: [[] as string[]]
  });

  readonly filtersForm = this.formBuilder.group({
    status: this.formBuilder.control<TicketStatus | null>(null),
    priority: this.formBuilder.control<TicketPriority | null>(null),
    projectId: this.formBuilder.nonNullable.control(''),
    assignedUserId: this.formBuilder.nonNullable.control('')
  });

  tickets: Ticket[] = [];
  projects: Project[] = [];
  developers: User[] = [];
  ticketPage: PagedResult<Ticket> = {
    items: [],
    pageNumber: 1,
    pageSize: this.defaultPageSize,
    totalCount: 0,
    totalPages: 0
  };
  isLoading = false;
  isSubmitting = false;
  deletingTicketId: string | null = null;
  loadErrorMessage = '';
  submitErrorMessage = '';
  editingTicketId: string | null = null;

  constructor(
    private readonly activatedRoute: ActivatedRoute,
    private readonly authService: AuthService,
    private readonly formBuilder: FormBuilder,
    private readonly projectsService: ProjectsService,
    private readonly router: Router,
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
    this.applyRouteFilters();
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.loadErrorMessage = '';

    forkJoin({
      projects: this.projectsService.getProjects(),
      tickets: this.ticketsService.getTickets(this.getTicketQuery()),
      developers: this.usersService.getDevelopers()
    })
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: ({ projects, tickets, developers }) => {
          this.projects = sortProjectsByName(projects);
          this.setTicketPage(tickets);
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
    const isEditOperation = this.editingTicketId !== null;

    const submitOperation = this.editingTicketId
      ? this.ticketsService.updateTicket(this.editingTicketId, this.toUpdateTicketRequest())
      : this.ticketsService.createTicket(this.toCreateTicketRequest(session.username));

    submitOperation.pipe(finalize(() => (this.isSubmitting = false))).subscribe({
      next: () => {
        this.resetForm();
        if (!isEditOperation) {
          this.ticketPage.pageNumber = 1;
        }

        this.loadTicketsPage();
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
          if (this.editingTicketId === ticket.id) {
            this.resetForm();
          }

          if (this.tickets.length === 1 && this.ticketPage.pageNumber > 1) {
            this.ticketPage.pageNumber -= 1;
          }

          this.loadTicketsPage();
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

  applyFilters(): void {
    this.ticketPage.pageNumber = 1;
    this.updateRouteFilters();
    this.loadTicketsPage();
  }

  clearFilters(): void {
    this.filtersForm.reset({
      status: null,
      priority: null,
      projectId: '',
      assignedUserId: ''
    });
    this.ticketPage.pageNumber = 1;
    this.ticketPage.pageSize = this.defaultPageSize;
    this.updateRouteFilters();
    this.loadTicketsPage();
  }

  onPageChange(event: PageEvent): void {
    this.ticketPage.pageNumber = event.pageIndex + 1;
    this.ticketPage.pageSize = event.pageSize;
    this.updateRouteFilters();
    this.loadTicketsPage();
  }

  private applyRouteFilters(): void {
    const queryParamMap = this.activatedRoute.snapshot.queryParamMap;

    this.filtersForm.reset({
      status: parseOptionalNumericQueryParam<TicketStatus>(queryParamMap.get('status')),
      priority: parseOptionalNumericQueryParam<TicketPriority>(queryParamMap.get('priority')),
      projectId: queryParamMap.get('projectId') ?? '',
      assignedUserId: queryParamMap.get('assignedUserId') ?? ''
    });

    this.ticketPage.pageNumber = parsePositiveInteger(queryParamMap.get('pageNumber')) ?? 1;
    this.ticketPage.pageSize = parsePositiveInteger(queryParamMap.get('pageSize')) ?? this.defaultPageSize;
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

  private loadTicketsPage(): void {
    this.isLoading = true;
    this.loadErrorMessage = '';

    this.ticketsService
      .getTickets(this.getTicketQuery())
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (ticketPage) => {
          this.setTicketPage(ticketPage);
        },
        error: () => {
          this.loadErrorMessage = 'Tickets could not be loaded right now. Try again.';
        }
      });
  }

  private getTicketQuery(): TicketQuery {
    const filters = this.filtersForm.getRawValue();

    return {
      status: filters.status ?? undefined,
      priority: filters.priority ?? undefined,
      projectId: filters.projectId || undefined,
      assignedUserId: filters.assignedUserId || undefined,
      pageNumber: this.ticketPage.pageNumber,
      pageSize: this.ticketPage.pageSize
    };
  }

  private setTicketPage(ticketPage: PagedResult<Ticket>): void {
    this.ticketPage = ticketPage;
    this.tickets = ticketPage.items;
  }

  private updateRouteFilters(): void {
    const query = this.getTicketQuery();

    void this.router.navigate([], {
      relativeTo: this.activatedRoute,
      queryParams: {
        status: query.status ?? null,
        priority: query.priority ?? null,
        projectId: query.projectId ?? null,
        assignedUserId: query.assignedUserId ?? null,
        pageNumber: query.pageNumber !== 1 ? query.pageNumber : null,
        pageSize: query.pageSize !== this.defaultPageSize ? query.pageSize : null
      }
    });
  }
}

function normalizeOptionalValue(value: string): string | null {
  const trimmedValue = value.trim();
  return trimmedValue.length > 0 ? trimmedValue : null;
}

function sortProjectsByName(projects: Project[]): Project[] {
  return [...projects].sort((left, right) => left.name.localeCompare(right.name));
}

function parseOptionalNumericQueryParam<T extends number>(value: string | null): T | null {
  if (!value) {
    return null;
  }

  const parsedValue = Number(value);
  return Number.isInteger(parsedValue) ? (parsedValue as T) : null;
}

function parsePositiveInteger(value: string | null): number | null {
  if (!value) {
    return null;
  }

  const parsedValue = Number(value);
  return Number.isInteger(parsedValue) && parsedValue > 0 ? parsedValue : null;
}

