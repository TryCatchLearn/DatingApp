import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { User } from '../models/user';
import { AccountService } from '../services/account.service';
import { take } from 'rxjs';

@Directive({
  selector: '[appHasRole]' // *appHasRole='["Admin", "Other"]'
})
export class HasRoleDirective implements OnInit {
  @Input()
  appHasRole:string[] = []

  user: User = {} as User

  constructor(
    private viewContainerRef: ViewContainerRef,
    private templateRef: TemplateRef<any>,
    private accountService: AccountService
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (!user) return

        this.user = user
      }
    })
  }

  ngOnInit(): void {
    if (this.user.roles.some(r => this.appHasRole.includes(r))) {
      this.viewContainerRef.createEmbeddedView(this.templateRef)
    } else {
      this.viewContainerRef.clear()
    }
  }

}
