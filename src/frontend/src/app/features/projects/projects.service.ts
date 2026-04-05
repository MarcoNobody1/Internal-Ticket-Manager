import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Project, ProjectDetails, SaveProjectRequest } from './project.models';

@Injectable({ providedIn: 'root' })
export class ProjectsService {
  constructor(private readonly httpClient: HttpClient) {}

  getProjects(): Observable<Project[]> {
    return this.httpClient.get<Project[]>('/api/projects');
  }

  getProjectById(projectId: string): Observable<ProjectDetails> {
    return this.httpClient.get<ProjectDetails>(`/api/projects/${projectId}`);
  }

  createProject(request: SaveProjectRequest): Observable<Project> {
    return this.httpClient.post<Project>('/api/projects', request);
  }

  updateProject(projectId: string, request: SaveProjectRequest): Observable<Project> {
    return this.httpClient.put<Project>(`/api/projects/${projectId}`, request);
  }

  deleteProject(projectId: string): Observable<void> {
    return this.httpClient.delete<void>(`/api/projects/${projectId}`);
  }
}
