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
