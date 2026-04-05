import { TicketAssignee, TicketPriority, TicketStatus } from '../tickets/ticket.models';

export interface Project {
  id: string;
  name: string;
  description: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface SaveProjectRequest {
  name: string;
  description: string | null;
}

export interface ProjectTicketSummary {
  id: string;
  title: string;
  status: TicketStatus;
  priority: TicketPriority;
  createdByUsername: string;
  updatedAtUtc: string;
  assignedDevelopers: TicketAssignee[];
}

export interface ProjectDetails {
  id: string;
  name: string;
  description: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  openTickets: ProjectTicketSummary[];
}
