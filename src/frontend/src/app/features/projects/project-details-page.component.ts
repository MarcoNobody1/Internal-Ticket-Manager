import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { getAssignedDeveloperNames as formatAssignedDeveloperNames, getTicketPriorityLabel, getTicketStatusLabel, TicketPriority, TicketStatus } from '../tickets/ticket.models';
import { ProjectDetails } from './project.models';
import { ProjectsService } from './projects.service';

@Component({
  selector: 'itm-project-details-page',
  standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule, MatCardModule, MatChipsModule, MatProgressSpinnerModule],
  templateUrl: './project-details-page.component.html',
  styleUrls: ['./project-details-page.component.css']
})
export class ProjectDetailsPageComponent implements OnInit {
  project: ProjectDetails | null = null;
  isLoading = true;
  loadErrorMessage = '';

  constructor(
    private readonly activatedRoute: ActivatedRoute,
    private readonly projectsService: ProjectsService
  ) {}

  ngOnInit(): void {
    const projectId = this.activatedRoute.snapshot.paramMap.get('projectId');

    if (!projectId) {
      this.isLoading = false;
      this.loadErrorMessage = 'Project could not be found.';
      return;
    }

    this.loadProject(projectId);
  }

  getStatusLabel(status: TicketStatus): string {
    return getTicketStatusLabel(status);
  }

  getPriorityLabel(priority: TicketPriority): string {
    return getTicketPriorityLabel(priority);
  }

  getAssignedDeveloperNames(ticketAssignees: ProjectDetails['openTickets'][number]['assignedDevelopers']): string {
    return formatAssignedDeveloperNames(ticketAssignees);
  }

  private loadProject(projectId: string): void {
    this.isLoading = true;
    this.loadErrorMessage = '';

    this.projectsService.getProjectById(projectId).subscribe({
      next: (project) => {
        this.project = project;
        this.isLoading = false;
      },
      error: () => {
        this.loadErrorMessage = 'Project details could not be loaded right now. Try again.';
        this.isLoading = false;
      }
    });
  }
}
