import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Member } from 'src/app/models/member';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  members$: Observable<Member[]> | undefined

  constructor(private memberService: MembersService) {

  }

  ngOnInit(): void {
    this.loadMembers()
  }

  loadMembers() {
    this.members$ = this.memberService.getMembers()
  }
}
