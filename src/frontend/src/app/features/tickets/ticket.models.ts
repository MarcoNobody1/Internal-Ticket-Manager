export type TicketStatus = 1 | 2 | 3 | 4;
export type TicketPriority = 1 | 2 | 3 | 4;

export interface TicketAssignee {
  id: string;
  username: string;
}

export interface Ticket {
  id: string;
  title: string;
  description: string | null;
  status: TicketStatus;
  priority: TicketPriority;
  projectId: string;
  assignedDevelopers: TicketAssignee[];
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

export interface CreateTicketRequest {
  title: string;
  description: string | null;
  status: TicketStatus;
  priority: TicketPriority;
  projectId: string;
  assignedDeveloperIds: string[];
  createdByUsername: string;
}

export interface UpdateTicketRequest {
  title: string;
  description: string | null;
  status: TicketStatus;
  priority: TicketPriority;
  projectId: string;
  assignedDeveloperIds: string[];
}

export const ticketStatusOptions: ReadonlyArray<{ value: TicketStatus; label: string }> = [
  { value: 1, label: 'Open' },
  { value: 2, label: 'In progress' },
  { value: 3, label: 'Resolved' },
  { value: 4, label: 'Closed' }
];

export const ticketPriorityOptions: ReadonlyArray<{ value: TicketPriority; label: string }> = [
  { value: 1, label: 'Low' },
  { value: 2, label: 'Medium' },
  { value: 3, label: 'High' },
  { value: 4, label: 'Critical' }
];

export function getTicketStatusLabel(status: TicketStatus): string {
  return ticketStatusOptions.find((option) => option.value === status)?.label ?? 'Unknown';
}

export function getTicketPriorityLabel(priority: TicketPriority): string {
  return ticketPriorityOptions.find((option) => option.value === priority)?.label ?? 'Unknown';
}

export function getAssignedDeveloperNames(assignedDevelopers: TicketAssignee[]): string {
  return assignedDevelopers.length > 0 ? assignedDevelopers.map((developer) => developer.username).join(', ') : 'Unassigned';
}
