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
});
