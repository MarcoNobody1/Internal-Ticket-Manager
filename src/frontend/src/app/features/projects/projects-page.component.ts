import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroupDirective, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';

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
  imports: [CommonModule, ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, MatTableModule],
  templateUrl: './projects-page.component.html',
  styleUrls: ['./projects-page.component.css']
})
export class ProjectsPageComponent implements OnInit {
  @ViewChild(FormGroupDirective) private projectFormDirective?: FormGroupDirective;

  readonly displayedColumns = ['name', 'description', 'updatedAtUtc', 'actions'];
  readonly projectNameMaxLength = projectNameMaxLength;
  readonly projectDescriptionMaxLength = projectDescriptionMaxLength;

  readonly projectForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(projectNameMaxLength), requiredTrimmedValidator]],
    description: ['', [Validators.maxLength(projectDescriptionMaxLength)]]
  });

  projects: Project[] = [];
  isLoadingProjects = false;
  isSubmitting = false;
  loadErrorMessage = '';
  submitErrorMessage = '';
  editingProjectId: string | null = null;

  constructor(private readonly formBuilder: FormBuilder, private readonly projectsService: ProjectsService) {}

  get isEditMode(): boolean {
    return this.editingProjectId !== null;
  }

  ngOnInit(): void {
    this.loadProjects();
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
    this.editingProjectId = project.id;
    this.submitErrorMessage = '';
    this.projectForm.setValue({
      name: project.name,
      description: project.description ?? ''
    });
  }

  cancelEdit(): void {
    this.resetForm();
  }

  submit(): void {
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
