import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';

import { Ticket } from './ticket.models';
import { TicketsService } from './tickets.service';

@Component({
  selector: 'itm-tickets-page',
  standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule, MatCardModule, MatChipsModule, MatProgressSpinnerModule],
  templateUrl: './tickets-page.component.html',
  styleUrls: ['./tickets-page.component.css']
})
export class TicketsPageComponent implements OnInit {
  tickets: Ticket[] = [];
  isLoadingTickets = false;
  loadErrorMessage = '';

  constructor(private readonly ticketsService: TicketsService) {}

  ngOnInit(): void {
    this.loadTickets();
  }

  loadTickets(): void {
    this.isLoadingTickets = true;
    this.loadErrorMessage = '';

    this.ticketsService
      .getTickets()
      .pipe(finalize(() => (this.isLoadingTickets = false)))
      .subscribe({
        next: (tickets) => {
          this.tickets = [...tickets].sort((left, right) => new Date(right.updatedAtUtc).getTime() - new Date(left.updatedAtUtc).getTime());
        },
        error: () => {
          this.loadErrorMessage = 'Tickets could not be loaded right now. Try again.';
        }
      });
  }

  trackByTicketId(_: number, ticket: Ticket): string {
    return ticket.id;
  }
}
