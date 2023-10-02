import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';
import { Photo } from '../models/photo';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl

  constructor(private http: HttpClient) { }

  getUsersWithRoles() {
    return this.http.get<User[]>(`${this.baseUrl}admin/users-with-roles`)
  }

  updateUserRoles(username: string, roles: string[]) {
    return this.http.post<string[]>(`${this.baseUrl}admin/edit-roles/${username}?roles=${roles}`, {})
  }

  getPhotosToModerate() {
    return this.http.get<Photo[]>(`${this.baseUrl}admin/photos-to-moderate`)
  }

  approvePhoto(photoId: number) {
    return this.http.put<Photo>(`${this.baseUrl}admin/approve-photo/${photoId}`, {})
  }

  deletePhoto(photoId: number) {
    return this.http.put<Photo>(`${this.baseUrl}admin/delete-photo/${photoId}`, {})
  }
}
