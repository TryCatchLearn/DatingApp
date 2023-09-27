import { CanActivateFn } from '@angular/router';
import { AccountService } from '../services/account.service';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = (route, state) => {
  const accountService: AccountService = inject(AccountService)
  const toastrService: ToastrService = inject(ToastrService)

  return accountService.currentUser$.pipe(
    map(user => {
      if (user) return true;

      toastrService.error('none shall pass!!!')
      return false;
    })
  );
};
