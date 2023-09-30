import { ResolveFn } from '@angular/router';
import { Member } from '../models/member';
import { inject } from '@angular/core';
import { MembersService } from '../services/members.service';
import { of } from 'rxjs';

export const memberDetailResolver: ResolveFn<Member | undefined> = (route, state) => {
  const memberService = inject(MembersService)
  const username = route.paramMap.get('username')
  if (!username) return of(undefined)

  return memberService.getMember(username)
};
