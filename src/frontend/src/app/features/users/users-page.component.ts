import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroupDirective, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';

import { SaveUserRequest, User, UserRole } from './user.models';
import { UsersService } from './users.service';

const usernameMaxLength = 100;
const roleOptions: UserRole[] = ['Admin', 'Developer'];

function requiredTrimmedValidator(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length > 0 ? null : { requiredTrimmed: true };
}

@Component({
  selector: 'itm-users-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatTableModule
  ],
  templateUrl: './users-page.component.html',
  styleUrls: ['./users-page.component.css']
})
export class UsersPageComponent implements OnInit {
  @ViewChild(FormGroupDirective) private userFormDirective?: FormGroupDirective;

  readonly displayedColumns = ['username', 'role', 'createdAtUtc', 'actions'];
  readonly usernameMaxLength = usernameMaxLength;
  readonly roleOptions = roleOptions;

  readonly userForm = this.formBuilder.nonNullable.group({
    username: ['', [Validators.required, Validators.maxLength(usernameMaxLength), requiredTrimmedValidator]],
    password: ['', [Validators.required, requiredTrimmedValidator]],
    role: ['Developer' as UserRole, [Validators.required]]
  });

  users: User[] = [];
  isLoadingUsers = false;
  isSubmitting = false;
  deletingUserId: string | null = null;
  loadErrorMessage = '';
  submitErrorMessage = '';
  deleteErrorMessage = '';
  editingUserId: string | null = null;

  constructor(private readonly formBuilder: FormBuilder, private readonly usersService: UsersService) {}

  get isEditMode(): boolean {
    return this.editingUserId !== null;
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoadingUsers = true;
    this.loadErrorMessage = '';

    this.usersService
      .getUsers()
      .pipe(finalize(() => (this.isLoadingUsers = false)))
      .subscribe({
        next: (users) => {
          this.users = sortUsersByUsername(users);
        },
        error: () => {
          this.loadErrorMessage = 'Users could not be loaded right now. Try again.';
        }
      });
  }

  startEdit(user: User): void {
    this.editingUserId = user.id;
    this.submitErrorMessage = '';
    this.deleteErrorMessage = '';
    this.setPasswordRequired(false);
    this.userForm.setValue({
      username: user.username,
      password: '',
      role: user.role
    });
  }

  cancelEdit(): void {
    this.resetForm();
  }

  submit(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.submitErrorMessage = '';

    const request = this.toSaveUserRequest();
    const submitOperation = this.editingUserId
      ? this.usersService.updateUser(this.editingUserId, request)
      : this.usersService.createUser(request);

    submitOperation.pipe(finalize(() => (this.isSubmitting = false))).subscribe({
      next: (user) => {
        this.users = upsertUser(this.users, user);
        this.resetForm();
      },
      error: (error: HttpErrorResponse) => {
        this.submitErrorMessage = getApiErrorMessage(
          error,
          this.editingUserId
            ? 'The user could not be updated. Review the form and try again.'
            : 'The user could not be created. Review the form and try again.'
        );
      }
    });
  }

  deleteUser(user: User): void {
    if (!window.confirm(`Delete ${user.username}?`)) {
      return;
    }

    this.deletingUserId = user.id;
    this.deleteErrorMessage = '';

    this.usersService
      .deleteUser(user.id)
      .pipe(finalize(() => (this.deletingUserId = null)))
      .subscribe({
        next: () => {
          this.users = this.users.filter((existingUser) => existingUser.id !== user.id);

          if (this.editingUserId === user.id) {
            this.resetForm();
          }
        },
        error: (error: HttpErrorResponse) => {
          this.deleteErrorMessage = getApiErrorMessage(error, 'The user could not be deleted right now.');
        }
      });
  }

  isDeleting(userId: string): boolean {
    return this.deletingUserId === userId;
  }

  trackByUserId(_: number, user: User): string {
    return user.id;
  }

  private resetForm(): void {
    this.editingUserId = null;
    this.submitErrorMessage = '';
    this.deleteErrorMessage = '';
    this.setPasswordRequired(true);
    this.userFormDirective?.resetForm({
      username: '',
      password: '',
      role: 'Developer'
    });
    this.userForm.reset({
      username: '',
      password: '',
      role: 'Developer'
    });
    this.userForm.markAsPristine();
    this.userForm.markAsUntouched();
    this.userForm.updateValueAndValidity({ emitEvent: false });
  }

  private setPasswordRequired(isRequired: boolean): void {
    const validators = isRequired ? [Validators.required, requiredTrimmedValidator] : [];
    this.userForm.controls.password.setValidators(validators);
    this.userForm.controls.password.updateValueAndValidity({ emitEvent: false });
  }

  private toSaveUserRequest(): SaveUserRequest {
    const formValue = this.userForm.getRawValue();
    const normalizedPassword = formValue.password.trim();

    return {
      username: formValue.username.trim(),
      password: normalizedPassword.length > 0 ? normalizedPassword : null,
      role: formValue.role
    };
  }
}

function sortUsersByUsername(users: User[]): User[] {
  return [...users].sort((left, right) => left.username.localeCompare(right.username));
}

function upsertUser(users: User[], savedUser: User): User[] {
  const remainingUsers = users.filter((user) => user.id !== savedUser.id);
  return sortUsersByUsername([...remainingUsers, savedUser]);
}

function getApiErrorMessage(error: HttpErrorResponse, fallbackMessage: string): string {
  const errors = error.error?.errors as Record<string, string[]> | undefined;
  const firstError = errors ? Object.values(errors).flat()[0] : undefined;
  return firstError || fallbackMessage;
}
