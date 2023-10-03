import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input, ViewChild } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { TimeagoModule } from 'ngx-timeago';
import { Message } from 'src/app/models/message';
import { MessageService } from 'src/app/services/message.service';

@Component({
  selector: 'app-member-messages',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
  imports: [ CommonModule, TimeagoModule, FormsModule ]
})
export class MemberMessagesComponent {
  @Input()
  username?: string

  messageContent = ''
  loading = false

  @ViewChild('messageForm')
  messageForm?: NgForm

  constructor(public messageService: MessageService) {

  }

  sendMessage() {
    if (!this.username) return

    this.loading = true
    this.messageService.sendMessage(this.username, this.messageContent)
      .then(() => {
        this.messageForm?.reset()
      })
      .finally(() => {
        this.loading = false
      })
  }
}
