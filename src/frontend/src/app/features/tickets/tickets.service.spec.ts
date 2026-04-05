import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { TicketsService } from './tickets.service';

describe('TicketsService', () => {
  let ticketsService: TicketsService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
    });

    ticketsService = TestBed.inject(TicketsService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('loads tickets with explicit filter and pagination query parameters', () => {
    let totalCount = 0;

    ticketsService
      .getTickets({
        status: 2,
        priority: 3,
        projectId: '8e3b7f77-2a07-4e8c-9b28-beb0176c2e06',
        assignedUserId: '11111111-1111-1111-1111-111111111111',
        pageNumber: 2,
        pageSize: 5
      })
      .subscribe((result) => {
        totalCount = result.totalCount;
        expect(result.items.length).toBe(1);
      });

    const request = httpTestingController.expectOne((pendingRequest) => pendingRequest.url === '/api/tickets');
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('status')).toBe('2');
    expect(request.request.params.get('priority')).toBe('3');
    expect(request.request.params.get('projectId')).toBe('8e3b7f77-2a07-4e8c-9b28-beb0176c2e06');
    expect(request.request.params.get('assignedUserId')).toBe('11111111-1111-1111-1111-111111111111');
    expect(request.request.params.get('pageNumber')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('5');

    request.flush({
      items: [
        {
          id: '5ab6d4cd-1f3d-4dc8-92af-e7d594cda111',
          title: 'Fix login form',
          description: 'The submit button stays disabled.',
          status: 2,
          priority: 3,
          projectId: '8e3b7f77-2a07-4e8c-9b28-beb0176c2e06',
          assignedDevelopers: [{ id: '11111111-1111-1111-1111-111111111111', username: 'developer.demo' }],
          createdByUsername: 'developer.demo',
          createdAtUtc: '2026-04-05T12:00:00Z',
          updatedAtUtc: '2026-04-05T12:00:00Z'
        }
      ],
      pageNumber: 2,
      pageSize: 5,
      totalCount: 7,
      totalPages: 2
    });

    expect(totalCount).toBe(7);
  });

  it('posts a comment to the nested ticket comments endpoint', () => {
    const ticketId = '8e3b7f77-2a07-4e8c-9b28-beb0176c2e06';
    let savedCommentId = '';

    ticketsService
      .createComment(ticketId, {
        authorUsername: 'developer.demo',
        content: 'I can reproduce this issue.'
      })
      .subscribe((comment) => {
        savedCommentId = comment.id;
        expect(comment.authorUsername).toBe('developer.demo');
      });

    const request = httpTestingController.expectOne(`/api/tickets/${ticketId}/comments`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      authorUsername: 'developer.demo',
      content: 'I can reproduce this issue.'
    });

    request.flush({
      id: '5ab6d4cd-1f3d-4dc8-92af-e7d594cda111',
      ticketId,
      authorUsername: 'developer.demo',
      content: 'I can reproduce this issue.',
      createdAtUtc: '2026-04-05T12:00:00Z'
    });

    expect(savedCommentId).toBe('5ab6d4cd-1f3d-4dc8-92af-e7d594cda111');
  });

  it('creates a ticket using the main tickets collection endpoint', () => {
    let createdTicketTitle = '';

    ticketsService
      .createTicket({
        title: 'Fix login form',
        description: 'The submit button stays disabled.',
        status: 1,
        priority: 3,
        projectId: '8e3b7f77-2a07-4e8c-9b28-beb0176c2e06',
        assignedDeveloperIds: ['11111111-1111-1111-1111-111111111111'],
        createdByUsername: 'developer.demo'
      })
      .subscribe((ticket) => {
        createdTicketTitle = ticket.title;
      });

    const request = httpTestingController.expectOne('/api/tickets');
    expect(request.request.method).toBe('POST');
    expect(request.request.body.createdByUsername).toBe('developer.demo');

    request.flush({
      id: '5ab6d4cd-1f3d-4dc8-92af-e7d594cda111',
      title: 'Fix login form',
      description: 'The submit button stays disabled.',
      status: 1,
      priority: 3,
      projectId: '8e3b7f77-2a07-4e8c-9b28-beb0176c2e06',
      assignedDevelopers: [{ id: '11111111-1111-1111-1111-111111111111', username: 'developer.demo' }],
      createdByUsername: 'developer.demo',
      createdAtUtc: '2026-04-05T12:00:00Z',
      updatedAtUtc: '2026-04-05T12:00:00Z'
    });

    expect(createdTicketTitle).toBe('Fix login form');
  });

  it('updates a ticket using the existing item route shape', () => {
    const ticketId = '8e3b7f77-2a07-4e8c-9b28-beb0176c2e06';
    let updatedTicketPriority = 0;

    ticketsService
      .updateTicket(ticketId, {
        title: 'Fix login form',
        description: 'Updated description',
        status: 2,
        priority: 4,
        projectId: '8e3b7f77-2a07-4e8c-9b28-beb0176c2e06',
        assignedDeveloperIds: ['22222222-2222-2222-2222-222222222222']
      })
      .subscribe((ticket) => {
        updatedTicketPriority = ticket.priority;
      });

    const request = httpTestingController.expectOne(`/api/tickets/${ticketId}`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body.priority).toBe(4);

    request.flush({
      id: ticketId,
      title: 'Fix login form',
      description: 'Updated description',
      status: 2,
      priority: 4,
      projectId: '8e3b7f77-2a07-4e8c-9b28-beb0176c2e06',
      assignedDevelopers: [{ id: '22222222-2222-2222-2222-222222222222', username: 'developer.ops' }],
      createdByUsername: 'developer.demo',
      createdAtUtc: '2026-04-05T12:00:00Z',
      updatedAtUtc: '2026-04-05T13:00:00Z'
    });

    expect(updatedTicketPriority).toBe(4);
  });

  it('deletes a ticket using the admin-only endpoint', () => {
    const ticketId = '8e3b7f77-2a07-4e8c-9b28-beb0176c2e06';
    let completed = false;

    ticketsService.deleteTicket(ticketId).subscribe(() => {
      completed = true;
    });

    const request = httpTestingController.expectOne(`/api/tickets/${ticketId}`);
    expect(request.request.method).toBe('DELETE');

    request.flush(null);

    expect(completed).toBeTrue();
  });
});
