import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { ProjectsService } from './projects.service';

describe('ProjectsService', () => {
  let projectsService: ProjectsService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
    });

    projectsService = TestBed.inject(ProjectsService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('loads the project list from the backend contract', () => {
    let projectsCount = 0;

    projectsService.getProjects().subscribe((projects) => {
      projectsCount = projects.length;
      expect(projects[0].name).toBe('Helpdesk Portal');
      expect(projects[0].updatedAtUtc).toBeNull();
    });

    const request = httpTestingController.expectOne('/api/projects');
    expect(request.request.method).toBe('GET');

    request.flush([
      {
        id: '8e3b7f77-2a07-4e8c-9b28-beb0176c2e06',
        name: 'Helpdesk Portal',
        description: 'Internal customer support workspace',
        createdAtUtc: '2026-04-03T12:00:00Z',
        updatedAtUtc: null
      }
    ]);

    expect(projectsCount).toBe(1);
  });

  it('updates a project using the existing backend route shape', () => {
    const projectId = '8e3b7f77-2a07-4e8c-9b28-beb0176c2e06';
    let updatedProjectName = '';

    projectsService
      .updateProject(projectId, {
        name: 'Helpdesk Portal v2',
        description: 'Updated description'
      })
      .subscribe((project) => {
        updatedProjectName = project.name;
      });

    const request = httpTestingController.expectOne(`/api/projects/${projectId}`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({
      name: 'Helpdesk Portal v2',
      description: 'Updated description'
    });

    request.flush({
      id: projectId,
      name: 'Helpdesk Portal v2',
      description: 'Updated description',
      createdAtUtc: '2026-04-03T12:00:00Z',
      updatedAtUtc: '2026-04-05T09:30:00Z'
    });

    expect(updatedProjectName).toBe('Helpdesk Portal v2');
  });

  it('loads project details with open tickets from the backend contract', () => {
    let openTicketCount = 0;

    projectsService.getProjectById('8e3b7f77-2a07-4e8c-9b28-beb0176c2e06').subscribe((project) => {
      openTicketCount = project.openTickets.length;
      expect(project.openTickets[0].assignedDevelopers[0].username).toBe('developer.demo');
    });

    const request = httpTestingController.expectOne('/api/projects/8e3b7f77-2a07-4e8c-9b28-beb0176c2e06');
    expect(request.request.method).toBe('GET');

    request.flush({
      id: '8e3b7f77-2a07-4e8c-9b28-beb0176c2e06',
      name: 'Helpdesk Portal',
      description: 'Internal customer support workspace',
      createdAtUtc: '2026-04-03T12:00:00Z',
      updatedAtUtc: '2026-04-05T09:30:00Z',
      openTickets: [
        {
          id: '5ab6d4cd-1f3d-4dc8-92af-e7d594cda111',
          title: 'Fix login form',
          status: 1,
          priority: 3,
          createdByUsername: 'developer.demo',
          updatedAtUtc: '2026-04-05T12:00:00Z',
          assignedDevelopers: [{ id: '11111111-1111-1111-1111-111111111111', username: 'developer.demo' }]
        }
      ]
    });

    expect(openTicketCount).toBe(1);
  });

  it('deletes a project using the admin route shape', () => {
    const projectId = '8e3b7f77-2a07-4e8c-9b28-beb0176c2e06';
    let completed = false;

    projectsService.deleteProject(projectId).subscribe(() => {
      completed = true;
    });

    const request = httpTestingController.expectOne(`/api/projects/${projectId}`);
    expect(request.request.method).toBe('DELETE');

    request.flush(null);

    expect(completed).toBeTrue();
  });
});
