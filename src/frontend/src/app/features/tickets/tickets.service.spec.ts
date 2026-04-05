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
});
