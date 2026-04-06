import { TicketPriority, TicketStatus } from '../../features/tickets/ticket.models';
import { UserRole } from '../../features/users/user.models';

export type UiSeverity = 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast';

export function getTicketStatusSeverity(status: TicketStatus): UiSeverity {
  switch (status) {
    case 1:
      return 'info';
    case 2:
      return 'warn';
    case 3:
      return 'success';
    case 4:
      return 'secondary';
    default:
      return 'secondary';
  }
}

export function getTicketPrioritySeverity(priority: TicketPriority): UiSeverity {
  switch (priority) {
    case 1:
      return 'secondary';
    case 2:
      return 'info';
    case 3:
      return 'warn';
    case 4:
      return 'danger';
    default:
      return 'secondary';
  }
}

export function getUserRoleSeverity(role: UserRole): UiSeverity {
  return role === 'Admin' ? 'danger' : 'info';
}
