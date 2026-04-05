import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { SaveUserRequest, User } from './user.models';

@Injectable({ providedIn: 'root' })
export class UsersService {
  constructor(private readonly httpClient: HttpClient) {}

  getUsers(): Observable<User[]> {
    return this.httpClient.get<User[]>('/api/users');
  }

  getDevelopers(): Observable<User[]> {
    return this.httpClient.get<User[]>('/api/users/developers');
  }

  getUserById(userId: string): Observable<User> {
    return this.httpClient.get<User>(`/api/users/${userId}`);
  }

  createUser(request: SaveUserRequest): Observable<User> {
    return this.httpClient.post<User>('/api/users', request);
  }

  updateUser(userId: string, request: SaveUserRequest): Observable<User> {
    return this.httpClient.put<User>(`/api/users/${userId}`, request);
  }

  deleteUser(userId: string): Observable<void> {
    return this.httpClient.delete<void>(`/api/users/${userId}`);
  }
}
