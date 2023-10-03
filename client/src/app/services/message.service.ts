import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getePaginationHeaders as getPaginationHeaders } from './paginationHelper';
import { Message } from '../models/message';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { User } from '../models/user';
import { BehaviorSubject, take } from 'rxjs';
import { Group } from '../models/group';
import { BusyService } from './busy.service';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private hubConnection?: HubConnection
  private messageThreadSource = new BehaviorSubject<Message[]>([])

  baseUrl = environment.apiUrl
  hubUrl = environment.hubUrl
  messageThread$ = this.messageThreadSource.asObservable()

  constructor(
    private http: HttpClient,
    private busyService: BusyService
  ) { }

  startConnection(user: User, otherUsername: string) {
    this.busyService.busy()
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubUrl}message?user=${otherUsername}`, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build()

    this.hubConnection
      .start()
      .catch(error => console.log(error))
      .finally(() => this.busyService.idle())

    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      if (group.connections.some(c => c.username === otherUsername)) {
        this.messageThread$.pipe(take(1)).subscribe({
          next: messages => {
            for (let message of messages) {
              if (!message.dateRead) {
                message.dateRead = new Date(Date.now())
              }
            }

            this.messageThreadSource.next([...messages])
          }
        })

      }
    })

    this.hubConnection.on('ReceiveMessageThread', messages => {
      this.messageThreadSource.next(messages)
    })

    this.hubConnection.on('NewMessage', message => {
      this.messageThread$.pipe(take(1)).subscribe({
        next: messages => {
          this.messageThreadSource.next([...messages, message])
        }
      })
    })

  }

  stopConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.messageThreadSource.next([])
      this.hubConnection.stop()
    }
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    const params = getPaginationHeaders(pageNumber, pageSize)
      .appendAll({ container })

    return getPaginatedResult<Message[]>(this.http, `${this.baseUrl}messages`, params)
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(`${this.baseUrl}messages/thread/${username}`)
  }

  async sendMessage(username: string, content: string) {
    return this.hubConnection?.invoke('SendMessage', { recipientUsername: username, content})
      .catch(error => console.log(error))
  }

  deleteMessage(id: number) {
    return this.http.delete(`${this.baseUrl}messages/${id}`)
  }
}
