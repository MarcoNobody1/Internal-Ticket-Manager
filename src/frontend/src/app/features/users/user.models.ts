export type UserRole = 'Admin' | 'Developer';

export interface User {
  id: string;
  username: string;
  role: UserRole;
  createdAtUtc: string;
}

export interface SaveUserRequest {
  username: string;
  password?: string | null;
  role: UserRole;
}
