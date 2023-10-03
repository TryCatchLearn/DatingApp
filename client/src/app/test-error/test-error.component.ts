import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-test-error',
  templateUrl: './test-error.component.html',
  styleUrls: ['./test-error.component.css']
})
export class TestErrorComponent {
  baseUrl = environment.apiUrl
  validationErrors: string[] = []

  constructor(
    private http: HttpClient
  ) {

  }

  get404Error() {
    this.http.get(this.baseUrl + 'buggy/not-found').subscribe({
      next: response => console.log({response}),
      error: response => console.log({response})
    })
  }

  get400Error() {
    this.http.get(this.baseUrl + 'buggy/bad-request').subscribe({
      next: response => console.log({response}),
      error: response => console.log({response})
    })
  }

  get500Error() {
    this.http.get(this.baseUrl + 'buggy/server-error').subscribe({
      next: response => console.log({response}),
      error: response => console.log({response})
    })
  }

  get401Error() {
    this.http.get(this.baseUrl + 'buggy/auth').subscribe({
      next: response => console.log({response}),
      error: response => console.log({response})
    })
  }

  get400ValidationError() {
    this.http.post(this.baseUrl + 'account/register', { password: '123' }).subscribe({
      next: response => console.log({response}),
      error: response => {
        this.validationErrors = response
        console.log({response})
      }
    })
  }
}
