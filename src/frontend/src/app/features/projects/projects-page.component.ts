import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroupDirective, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { Textarea } from 'primeng/inputtextarea';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { finalize } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { Project, SaveProjectRequest } from './project.models';
import { ProjectsService } from './projects.service';

const projectNameMaxLength = 200;
const projectDescriptionMaxLength = 2000;

function requiredTrimmedValidator(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length > 0 ? null : { requiredTrimmed: true };
}

@Component({
  selector: 'itm-projects-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ButtonModule, CardModule, InputTextModule, Textarea, ProgressSpinnerModule, TableModule],
  templateUrl: './projects-page.component.html',
  styleUrls: ['./projects-page.component.css']
})
export class ProjectsPageComponent implements OnInit {
  @ViewChild(FormGroupDirective) private projectFormDirective?: FormGroupDirective;

  readonly projectNameMaxLength = projectNameMaxLength;
  readonly projectDescriptionMaxLength = projectDescriptionMaxLength;

  readonly projectForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(projectNameMaxLength), requiredTrimmedValidator]],
    description: ['', [Validators.maxLength(projectDescriptionMaxLength)]]
  });

  projects: Project[] = [];
  isLoadingProjects = false;
  isSubmitting = false;
  deletingProjectId: string | null = null;
  loadErrorMessage = '';
  submitErrorMessage = '';
  editingProjectId: string | null = null;
  projectsTableStacked = typeof window !== 'undefined' ? window.innerWidth <= 1100 : false;

  constructor(
    private readonly authService: AuthService,
    private readonly formBuilder: FormBuilder,
    private readonly projectsService: ProjectsService
  ) {}

  get canManageProjects(): boolean {
    return this.authService.hasRole('Admin');
  }

  get displayedColumns(): string[] {
    return this.canManageProjects ? ['name', 'description', 'updatedAtUtc', 'actions'] : ['name', 'description', 'updatedAtUtc', 'details'];
  }

  get isEditMode(): boolean {
    return this.editingProjectId !== null;
  }

  get formTitle(): string {
    return this.isEditMode ? 'Edit project' : 'Create project';
  }

  ngOnInit(): void {
    this.loadProjects();
  }

  @HostListener('window:resize')
  onWindowResize(): void {
    this.projectsTableStacked = window.innerWidth <= 1100;
  }

  loadProjects(): void {
    this.isLoadingProjects = true;
    this.loadErrorMessage = '';

    this.projectsService
      .getProjects()
      .pipe(finalize(() => (this.isLoadingProjects = false)))
      .subscribe({
        next: (projects) => {
          this.projects = sortProjectsByName(projects);
        },
        error: () => {
          this.loadErrorMessage = 'Projects could not be loaded right now. Try again.';
        }
      });
  }

  startEdit(project: Project): void {
    if (!this.canManageProjects) {
      return;
    }

    this.editingProjectId = project.id;
    this.submitErrorMessage = '';
    this.projectForm.setValue({
      name: project.name,
      description: project.description ?? ''
    });
  }

  cancelEdit(): void {
    if (!this.canManageProjects) {
      return;
    }

    this.resetForm();
  }

  submit(): void {
    if (!this.canManageProjects) {
      return;
    }

    if (this.projectForm.invalid) {
      this.projectForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.submitErrorMessage = '';

    const request = this.toSaveProjectRequest();
    const submitOperation = this.editingProjectId
      ? this.projectsService.updateProject(this.editingProjectId, request)
      : this.projectsService.createProject(request);

    submitOperation.pipe(finalize(() => (this.isSubmitting = false))).subscribe({
      next: (project) => {
        this.projects = upsertProject(this.projects, project);
        this.resetForm();
      },
      error: () => {
        this.submitErrorMessage = this.editingProjectId
          ? 'The project could not be updated. Review the form and try again.'
          : 'The project could not be created. Review the form and try again.';
      }
    });
  }

  deleteProject(project: Project): void {
    if (!this.canManageProjects || this.deletingProjectId) {
      return;
    }

    const confirmed = window.confirm(`Delete project "${project.name}"? This also removes its tickets.`);
    if (!confirmed) {
      return;
    }

    this.deletingProjectId = project.id;
    this.submitErrorMessage = '';

    this.projectsService
      .deleteProject(project.id)
      .pipe(finalize(() => (this.deletingProjectId = null)))
      .subscribe({
        next: () => {
          this.projects = this.projects.filter((existingProject) => existingProject.id !== project.id);

          if (this.editingProjectId === project.id) {
            this.resetForm();
          }
        },
        error: () => {
          this.submitErrorMessage = 'The project could not be deleted right now. Try again.';
        }
      });
  }

  trackByProjectId(_: number, project: Project): string {
    return project.id;
  }

  private resetForm(): void {
    this.editingProjectId = null;
    this.submitErrorMessage = '';
    this.projectFormDirective?.resetForm({
      name: '',
      description: ''
    });
    this.projectForm.reset({
      name: '',
      description: ''
    });
    this.projectForm.markAsPristine();
    this.projectForm.markAsUntouched();
    this.projectForm.updateValueAndValidity({ emitEvent: false });
  }

  private toSaveProjectRequest(): SaveProjectRequest {
    const formValue = this.projectForm.getRawValue();
    const normalizedDescription = formValue.description.trim();

    return {
      name: formValue.name.trim(),
      description: normalizedDescription.length > 0 ? normalizedDescription : null
    };
  }
}

function sortProjectsByName(projects: Project[]): Project[] {
  return [...projects].sort((left, right) => left.name.localeCompare(right.name));
}

function upsertProject(projects: Project[], savedProject: Project): Project[] {
  const remainingProjects = projects.filter((project) => project.id !== savedProject.id);
  return sortProjectsByName([...remainingProjects, savedProject]);
}
