import { Component, Input, OnInit } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { take } from 'rxjs';
import { Member } from 'src/app/models/member';
import { Photo } from 'src/app/models/photo';
import { User } from 'src/app/models/user';
import { AccountService } from 'src/app/services/account.service';
import { MembersService } from 'src/app/services/members.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input()
  member?: Member

  uploader?: FileUploader
  hasBaseDropZoneOver = false
  baseUrl = environment.apiUrl
  user?: User

  constructor(
    private accountService: AccountService,
    private memberService: MembersService
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) this.user = user
      }
    })
  }

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(e: any) {
    this.hasBaseDropZoneOver = e
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: `${this.baseUrl}users/add-photo`,
      authToken: `Bearer ${this.user?.token}`,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    })

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false
    }

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const photo: Photo = JSON.parse(response)
        this.member?.photos.push(photo)
        if (photo.isMain) {
          if (this.member) this.member.photoUrl = photo.url
          if (this.user) this.user.photoUrl = photo.url
        }
      }
    }
  }

  setMainPhoto(photo: Photo) {
    this.memberService.setMainPhoto(photo.id).subscribe({
      next: () => {
        if (this.user && this.member) {
          this.user.photoUrl = photo.url
          this.accountService.setCurrentUser(this.user)
          this.member.photoUrl = photo.url
          this.member.photos.forEach(p => p.isMain = p.id == photo.id)
        }
      }
    })
  }

  deletePhoto(photoId: number) {
    this.memberService.deletePhoto(photoId).subscribe({
      next: _ => {
        if (this.member) {
          this.member.photos = this.member.photos.filter(p => p.id !== photoId)
        }
      }
    })
  }
}
