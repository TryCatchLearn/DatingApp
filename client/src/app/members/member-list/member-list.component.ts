import { Component, OnInit } from '@angular/core';
import { PageChangedEvent } from 'ngx-bootstrap/pagination';
import { Member } from 'src/app/models/member';
import { Pagination } from 'src/app/models/pagination';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  members: Member[] = []
  pagination?: Pagination
  genderList = [{ value: 'male', display: 'Male'}, {value: 'female', display: 'Female'}]

  constructor(
    private memberService: MembersService
  ) {

  }

  ngOnInit(): void {
    this.loadMembers()
  }

  get userParams() {
    return this.memberService.userParams
  }

  set userParams(userParams) {
    this.memberService.userParams = userParams
  }

  loadMembers() {
    this.memberService.getMembers().subscribe({
      next: response => {
        if (response) {
          this.members = response.result
          this.pagination = { ...response.pagination }
        }
      }
    })
  }

  pageChanged(event: PageChangedEvent) {
    if (!this.userParams) return
    if (this.userParams.pageNumber === event.page) return

    this.userParams.pageNumber = event.page
    this.loadMembers()
  }

  applyFilters() {
    this.userParams.pageNumber = 1
    this.loadMembers();
  }

  resetFilters() {
    this.memberService.resetUserParams()
    this.loadMembers()
  }
}
