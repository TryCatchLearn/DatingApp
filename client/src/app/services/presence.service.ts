import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';
import { BehaviorSubject, take } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  private hubConnection?: HubConnection
  private usersOnlineSource = new BehaviorSubject<string[]>([])

  hubUrl = environment.hubUrl
  usersOnline$ = this.usersOnlineSource.asObservable()

  constructor(
    private toastrService: ToastrService,
    private router: Router
  ) { }

  startHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubUrl}presence`, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build()

    this.hubConnection.start().catch(error => console.log(error))

    this.hubConnection.on('UserIsOnline', username => {
      this.usersOnline$.pipe(take(1)).subscribe({
        next: usernames => this.usersOnlineSource.next([...usernames, username])
      })
    })

    this.hubConnection.on('UserIsOffline', username => {
      this.usersOnline$.pipe(take(1)).subscribe({
        next: usernames => this.usersOnlineSource.next(usernames.filter(u => u !== username))
      })
    })

    this.hubConnection.on('UsersOnline', usernames => {
      this.usersOnlineSource.next(usernames)
    })

    this.hubConnection.on('NewMessageReceived', ({username, knownAs}) => {
      this.toastrService
        .info(`${knownAs} sent you a message! Click to view`)
        .onTap.pipe(take(1)).subscribe({
          next: () => this.router.navigateByUrl(`/members/${username}?tab=Messages`)
        })
    })
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch(error => console.log(error))
  }
}
