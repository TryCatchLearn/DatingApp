import { Component, OnInit } from '@angular/core';
import { Member } from '../models/member';
import { MembersService } from '../services/members.service';
import { Pagination } from '../models/pagination';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
  members: Member[] = []
  pagination!: Pagination
  predicate = 'liked'
  pageNumber = 1
  pageSize = 5

  constructor(private memberService: MembersService) {
  }

  ngOnInit(): void {
    this.loadLikes()
  }

  loadLikes() {
    this.memberService.getLikes(this.predicate, this.pageNumber, this.pageSize).subscribe({
      next: response => {
        this.members = response.result
        this.pagination = response.pagination
      }
    })
  }

  pageChanged(event: any) {
    if (this.pageNumber === event.page) return;

    this.pageNumber = event.page
    this.loadLikes()
  }
}
