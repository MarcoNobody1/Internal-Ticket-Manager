import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize, switchMap } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { Ticket } from '../tickets/ticket.models';
import { TicketsService } from '../tickets/tickets.service';
import { UsersService } from '../users/users.service';

@Component({
  selector: 'itm-account-page',
  standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule, MatCardModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './account-page.component.html',
  styleUrls: ['./account-page.component.css']
})
export class AccountPageComponent implements OnInit {
  readonly session = this.authService.getSession();

  assignedTickets: Ticket[] = [];
  assignedDeveloperId: string | null = null;
  assignedTicketsErrorMessage = '';
  isLoadingAssignedTickets = false;

  constructor(
    private readonly authService: AuthService,
    private readonly ticketsService: TicketsService,
    private readonly usersService: UsersService
  ) {}

  get isDeveloper(): boolean {
    return this.session?.role === 'Developer';
  }

  ngOnInit(): void {
    if (!this.session || !this.isDeveloper) {
      return;
    }

    this.isLoadingAssignedTickets = true;
    this.usersService
      .getDevelopers()
      .pipe(
        switchMap((developers) => {
          const currentDeveloper = developers.find((developer) => developer.username === this.session?.username);

          if (!currentDeveloper) {
            throw new Error('Current developer was not found in the developers list.');
          }

          this.assignedDeveloperId = currentDeveloper.id;

          return this.ticketsService.getTickets({
            assignedUserId: currentDeveloper.id,
            pageNumber: 1,
            pageSize: 5
          });
        }),
        finalize(() => (this.isLoadingAssignedTickets = false))
      )
      .subscribe({
        next: (ticketPage) => {
          this.assignedTickets = ticketPage.items;
        },
        error: () => {
          this.assignedTicketsErrorMessage = 'Assigned tickets could not be loaded right now. Try again from the tickets screen.';
        }
      });
  }
}
