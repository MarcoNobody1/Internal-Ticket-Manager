import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroupDirective, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { forkJoin } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { getAssignedDeveloperNames as formatAssignedDeveloperNames, getTicketPriorityLabel, getTicketStatusLabel, Ticket, TicketComment, TicketPriority, TicketStatus } from './ticket.models';
import { TicketsService } from './tickets.service';

const commentMaxLength = 2000;

function requiredTrimmedValidator(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length > 0 ? null : { requiredTrimmed: true };
}

@Component({
  selector: 'itm-ticket-details-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatButtonModule, MatCardModule, MatChipsModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule],
  templateUrl: './ticket-details-page.component.html',
  styleUrls: ['./ticket-details-page.component.css']
})
export class TicketDetailsPageComponent implements OnInit {
  @ViewChild(FormGroupDirective) private commentFormDirective?: FormGroupDirective;

  readonly commentMaxLength = commentMaxLength;
  readonly commentForm = this.formBuilder.nonNullable.group({
    authorUsername: [{ value: this.authService.getSession()?.username ?? '', disabled: true }, [Validators.required]],
    content: ['', [Validators.required, Validators.maxLength(commentMaxLength), requiredTrimmedValidator]]
  });

  ticket: Ticket | null = null;
  comments: TicketComment[] = [];
  isLoading = true;
  isSubmitting = false;
  loadErrorMessage = '';
  submitErrorMessage = '';

  constructor(
    private readonly activatedRoute: ActivatedRoute,
    private readonly authService: AuthService,
    private readonly formBuilder: FormBuilder,
    private readonly ticketsService: TicketsService
  ) {}

  ngOnInit(): void {
    const ticketId = this.activatedRoute.snapshot.paramMap.get('ticketId');

    if (!ticketId) {
      this.isLoading = false;
      this.loadErrorMessage = 'Ticket could not be found.';
      return;
    }

    this.loadTicketDetails(ticketId);
  }

  submitComment(): void {
    if (this.commentForm.invalid || !this.ticket) {
      this.commentForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.submitErrorMessage = '';

    const formValue = this.commentForm.getRawValue();

    this.ticketsService
      .createComment(this.ticket.id, {
        authorUsername: formValue.authorUsername.trim(),
        content: formValue.content.trim()
      })
      .subscribe({
        next: (comment) => {
          this.comments = [...this.comments, comment];
          this.ticket = {
            ...this.ticket!,
            updatedAtUtc: comment.createdAtUtc
          };
          this.resetCommentForm();
          this.isSubmitting = false;
        },
        error: () => {
          this.submitErrorMessage = 'Comment could not be saved. Review the form and try again.';
          this.isSubmitting = false;
        }
      });
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

  private loadTicketDetails(ticketId: string): void {
    this.isLoading = true;
    this.loadErrorMessage = '';

    forkJoin({
      ticket: this.ticketsService.getTicketById(ticketId),
      comments: this.ticketsService.getComments(ticketId)
    }).subscribe({
      next: ({ ticket, comments }) => {
        this.ticket = ticket;
        this.comments = comments;
        this.isLoading = false;
      },
      error: () => {
        this.loadErrorMessage = 'Ticket details could not be loaded right now. Try again.';
        this.isLoading = false;
      }
    });
  }

  private resetCommentForm(): void {
    this.commentFormDirective?.resetForm({
      authorUsername: this.authService.getSession()?.username ?? '',
      content: ''
    });

    this.commentForm.reset({
      authorUsername: this.authService.getSession()?.username ?? '',
      content: ''
    });

    this.commentForm.markAsPristine();
    this.commentForm.markAsUntouched();
    this.commentForm.updateValueAndValidity({ emitEvent: false });
  }
}
