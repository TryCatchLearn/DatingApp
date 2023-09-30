import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment'
import { Member } from 'src/app/models/member'
import { map, of, take } from 'rxjs';
import { PaginatedResult } from '../models/pagination';
import { UserParams } from '../models/userParams';
import { AccountService } from './account.service';
import { getPaginatedResult, getePaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl
  memberCache = new Map<string, PaginatedResult<Member[]>>()
  userParams!: UserParams

  constructor(
    private http: HttpClient,
    private accountService: AccountService
  ) {
    this.accountService.currentUser$.subscribe({
      next: user => {
        if (user) {
          this.userParams = new UserParams(user)
        }
      }
    })
    this.resetUserParams()
  }

  resetUserParams() {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) {
          this.userParams = new UserParams(user)
        }
      }
    })
  }

  getMembers() {
    const cacheKey = Object.values(this.userParams).join('-')
    if (this.memberCache.has(cacheKey)) return of(this.memberCache.get(cacheKey))

    const params = new HttpParams({ fromObject: {...this.userParams} })

    return getPaginatedResult<Member[]>(this.http, `${this.baseUrl}users`, params).pipe(
      map(response => {
        this.memberCache.set(cacheKey, response)
        return response
      })
    )
  }

  getMember(username: string) {
    const members = [...this.memberCache.values()].reduce<Member[]>(
      (a, e) => a.concat(e.result), []
    )
    const member = members.find(m => m.userName === username)
    if (member) return of(member)

    return this.http.get<Member>(`${this.baseUrl}users/${username}`)
  }

  updateMember(member: Member) {
    return this.http.put(`${this.baseUrl}users`, member)
  }

  setMainPhoto(photoId: number) {
    return this.http.put(`${this.baseUrl}users/set-main-photo/${photoId}`, {})
  }

  deletePhoto(photoId: number) {
    return this.http.delete(`${this.baseUrl}users/delete-photo/${photoId}`)
  }

  addLike(username: string) {
    return this.http.post(`${this.baseUrl}likes/${username}`, {})
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    const params = getePaginationHeaders(pageNumber, pageSize)
      .appendAll({ predicate })

    return getPaginatedResult<Member[]>(this.http, `${this.baseUrl}likes`, params)
  }
}
