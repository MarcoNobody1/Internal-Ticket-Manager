import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { CreateTicketCommentRequest, Ticket, TicketComment } from './ticket.models';

@Injectable({ providedIn: 'root' })
export class TicketsService {
  constructor(private readonly httpClient: HttpClient) {}

  getTickets(): Observable<Ticket[]> {
    return this.httpClient.get<Ticket[]>('/api/tickets');
  }

  getTicketById(ticketId: string): Observable<Ticket> {
    return this.httpClient.get<Ticket>(`/api/tickets/${ticketId}`);
  }

  getComments(ticketId: string): Observable<TicketComment[]> {
    return this.httpClient.get<TicketComment[]>(`/api/tickets/${ticketId}/comments`);
  }

  createComment(ticketId: string, request: CreateTicketCommentRequest): Observable<TicketComment> {
    return this.httpClient.post<TicketComment>(`/api/tickets/${ticketId}/comments`, request);
  }
}
