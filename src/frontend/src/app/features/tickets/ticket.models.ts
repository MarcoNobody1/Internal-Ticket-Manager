export interface Ticket {
  id: string;
  title: string;
  description: string | null;
  status: string;
  priority: string;
  projectId: string;
  assignedUserId: string | null;
  createdByUsername: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface TicketComment {
  id: string;
  ticketId: string;
  authorUsername: string;
  content: string;
  createdAtUtc: string;
}

export interface CreateTicketCommentRequest {
  authorUsername: string;
  content: string;
}
