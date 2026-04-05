import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { CreateTicketCommentRequest, CreateTicketRequest, PagedResult, Ticket, TicketComment, TicketQuery, UpdateTicketRequest } from './ticket.models';

@Injectable({ providedIn: 'root' })
export class TicketsService {
  constructor(private readonly httpClient: HttpClient) {}

  getTickets(query: TicketQuery): Observable<PagedResult<Ticket>> {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize);

    if (query.status !== undefined) {
      params = params.set('status', query.status);
    }

    if (query.priority !== undefined) {
      params = params.set('priority', query.priority);
    }

    if (query.projectId) {
      params = params.set('projectId', query.projectId);
    }

    if (query.assignedUserId) {
      params = params.set('assignedUserId', query.assignedUserId);
    }

    return this.httpClient.get<PagedResult<Ticket>>('/api/tickets', { params });
  }

  getTicketById(ticketId: string): Observable<Ticket> {
    return this.httpClient.get<Ticket>(`/api/tickets/${ticketId}`);
  }

  getComments(ticketId: string): Observable<TicketComment[]> {
    return this.httpClient.get<TicketComment[]>(`/api/tickets/${ticketId}/comments`);
  }

  createTicket(request: CreateTicketRequest): Observable<Ticket> {
    return this.httpClient.post<Ticket>('/api/tickets', request);
  }

  updateTicket(ticketId: string, request: UpdateTicketRequest): Observable<Ticket> {
    return this.httpClient.put<Ticket>(`/api/tickets/${ticketId}`, request);
  }

  deleteTicket(ticketId: string): Observable<void> {
    return this.httpClient.delete<void>(`/api/tickets/${ticketId}`);
  }

  createComment(ticketId: string, request: CreateTicketCommentRequest): Observable<TicketComment> {
    return this.httpClient.post<TicketComment>(`/api/tickets/${ticketId}/comments`, request);
  }
}
