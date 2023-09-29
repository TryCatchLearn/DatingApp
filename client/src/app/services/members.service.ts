import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment'
import { Member } from 'src/app/models/member'
import { map, of, take } from 'rxjs';
import { PaginatedResult } from '../models/pagination';
import { UserParams } from '../models/userParams';
import { AccountService } from './account.service';
import { User } from '../models/user';

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

    return this.getPaginatedResult<Member[]>(`${this.baseUrl}users`, params).pipe(
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
    // .pipe(
    //   map(() => {
    //     const index = this.members.indexOf(member)
    //     this.members[index] = {...this.members[index], ...member}
    //   })
    // )
  }

  setMainPhoto(photoId: number) {
    return this.http.put(`${this.baseUrl}users/set-main-photo/${photoId}`, {})
  }

  deletePhoto(photoId: number) {
    return this.http.delete(`${this.baseUrl}users/delete-photo/${photoId}`)
  }

  private getPaginatedResult<T>(url: string, params: HttpParams) {
    const paginatedResult = new PaginatedResult<T>

    return this.http.get<T>(url, { observe: 'response', params }).pipe(
      map(response => {
        if (response.body) {
          paginatedResult.result = response.body;
        }

        const pagination = response.headers.get('Pagination');
        if (pagination) {
          paginatedResult.pagination = JSON.parse(pagination);
        }

        return paginatedResult;
      })
    );
  }

}
