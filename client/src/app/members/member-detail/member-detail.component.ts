import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TabDirective, TabsModule, TabsetComponent } from 'ngx-bootstrap/tabs';
import { TimeagoModule } from 'ngx-timeago';
import { Member } from 'src/app/models/member';
import { MemberMessagesComponent } from '../member-messages/member-messages.component';
import { MessageService } from 'src/app/services/message.service';
import { Message } from 'src/app/models/message';
import { PresenceService } from 'src/app/services/presence.service';
import { AccountService } from 'src/app/services/account.service';
import { take } from 'rxjs';
import { User } from 'src/app/models/user';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
  imports: [CommonModule, TabsModule, GalleryModule, TimeagoModule, MemberMessagesComponent]
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  member: Member = {} as Member
  images: GalleryItem[] = []
  activeTab?: TabDirective
  messages: Message[] = []
  user?: User

  @ViewChild('memberTabs', {static: true})
  memberTabs?: TabsetComponent

  constructor(
    private route: ActivatedRoute,
    private accountService: AccountService,
    private messageService: MessageService,
    public presenceService: PresenceService
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) this.user = user
      }
    })
  }

  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => this.member = data['member']
    })

    this.route.queryParams.subscribe({
      next: params => {
        if (params['tab']) {
          this.selectTab(params['tab'])
        }
      }
    })

    this.getImages()
  }

  ngOnDestroy(): void {
    this.messageService.stopConnection();
  }

  selectTab(heading: string) {
    const tab = this.memberTabs?.tabs.find(t => t.heading === heading)
    if (tab) {
      tab.active = true
    }
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data
    if (this.activeTab.heading === 'Messages' && this.user) {
      this.messageService.startConnection(this.user, this.member.userName)
    } else {
      this.messageService.stopConnection()
    }
  }

  loadMessages() {
    if (!this.member) return

    this.messageService.getMessageThread(this.member?.userName).subscribe({
      next: messages => this.messages = messages
    })
  }

  getImages() {
    if (!this.member) return;

    for (const photo of this.member.photos) {
      this.images.push(new ImageItem({ src: photo.url, thumb: photo.url }))
    }
  }
}
