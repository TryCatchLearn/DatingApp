import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getePaginationHeaders as getPaginationHeaders } from './paginationHelper';
import { Message } from '../models/message';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl

  constructor(
    private http: HttpClient
  ) { }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    const params = getPaginationHeaders(pageNumber, pageSize)
      .appendAll({ container })

    return getPaginatedResult<Message[]>(this.http, `${this.baseUrl}messages`, params)
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(`${this.baseUrl}messages/thread/${username}`)
  }

  sendMessage(username: string, content: string) {
    return this.http.post<Message>(`${this.baseUrl}messages`, {recipientUserName: username, content})
  }

  deleteMessage(id: number) {
    return this.http.delete(`${this.baseUrl}messages/${id}`)
  }
}
