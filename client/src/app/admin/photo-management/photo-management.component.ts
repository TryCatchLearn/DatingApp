import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Photo } from 'src/app/models/photo';
import { AdminService } from 'src/app/services/admin.service';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[] = []

  constructor(
    private adminService: AdminService,
    private toastrService: ToastrService
  ) {
  }

  ngOnInit(): void {
    this.loadPhotos()
  }

  loadPhotos() {
    this.adminService.getPhotosToModerate().subscribe({
      next: photos => {
        this.photos = photos
      }
    })
  }

  approvePhoto(photoId: number) {
    this.adminService.approvePhoto(photoId).subscribe({
      next: _ => {
        this.photos = this.photos.filter(p => p.id != photoId)
        this.toastrService.success('Photo was approved')
      }
    })
  }

  deletePhoto(photoId: number) {
    this.adminService.deletePhoto(photoId).subscribe({
      next: _ => {
        this.photos = this.photos.filter(p => p.id != photoId)
        this.toastrService.info('Photo was deleted')
      }
    })
  }

}
