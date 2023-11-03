import { HttpClient, HttpEvent, HttpEventType } from '@angular/common/http';
import { Component, Inject, OnInit, TemplateRef } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Subscription } from 'rxjs';

import { AccountModel } from '../Entity/AccountModel';
import { CategoryModel } from '../Entity/CategoryModel';
import { FileInfoModel } from '../Entity/FileInfoModel';
import { ResponseModel } from '../Entity/ResponseModel';

import { FilesHandler } from '../Handlers/FilesHandler';
import { AccountService } from '../services/account.service';
import { CategoryService } from '../services/category.service';
import { FilesService } from '../services/files.service';

@Component({
  selector: 'app-files',
  templateUrl: './files.component.html',
  styleUrls: ['./files.component.css']
})
export class FilesComponent implements OnInit {
  page: number = 0;
  baseUrl: string;
  fileForm = new FormGroup({
  });
  fileToUpload!: File;
  searchValue: string = '';
  searchActive: boolean;
  uploadActive: boolean;
  currentAccount!: AccountModel;
  modalRef?: BsModalRef;
  modalCategyPanelRef?: BsModalRef;
  modalService: BsModalService;
  categoryPanelActive: boolean = false;
  categories: CategoryModel[] = [];
  currentCategory: CategoryModel | null = null;
  fileHandler: FilesHandler;
  currentSubscription!: Subscription;
  uploadProgress: number = 0;
  uploadedFile!: FileInfoModel | null;

  constructor(@Inject('BASE_URL') url: string, private accountService: AccountService,
    private fileService: FilesService, private router: Router, private ModalService: BsModalService, private categoryService: CategoryService) {
    this.baseUrl = url;
    this.searchActive = false;
    this.uploadActive = false;
    this.modalService = ModalService;
    this.fileHandler = new FilesHandler(fileService);
  }

  ngOnInit() {
    this.fileService.equalizeFileServerWithDB().subscribe(x => console.log(x));
    this.getFiles();
    this.accountService.currentAccount$.subscribe(res => {
      this.currentAccount = res as AccountModel;
      this.fillCategories();
    });
  }

  getFiles() {
    this.fileHandler.setFileList();
  }

  fillCategories() {
    this.categoryService.getCategories().subscribe(x => {
      if (x.isSuccessful) {
        x.data.forEach((y: CategoryModel) => this.categories.push(y));
      }
    });
  }

  selectAllCategories() {
    this.currentCategory = null;
    this.fileHandler.setCategoryFileList(null);
    this.modalCategyPanelRef?.hide();
  }

  selectCategory(category: CategoryModel) {
    if (category === null)
    {
      this.fileHandler.setCategoryFileList(null);
      this.modalCategyPanelRef?.hide();
      return;
    }
    this.currentCategory = category;
    this.fileHandler.setCategoryFileList(this.currentCategory);
    this.modalCategyPanelRef?.hide();
  }

  setCategoryUnsorted(fileId: number) {
    var unsortedCategory = this.categories.find(x => x.title === 'Unsorted') as CategoryModel;
    this.setCategory(unsortedCategory);
  }

  setCategory(category: CategoryModel) {
    var file = this.fileHandler.fileList.find(x => x.id === this.fileHandler.selectedFile?.id) as FileInfoModel;
    file.category_Id = category.id;
    file.categoryTitle = category.title;
    return this.fileService.updateFileCategory(file).subscribe(x => {
      if (x.isSuccessful) {
        if (this.fileHandler.selectedFile != null)
          this.fileHandler.selectedFile = x.data;

        var index = this.fileHandler.fileList.findIndex(y => y.id === x.data.id)
        this.fileHandler.fileList[index] = x.data;
        this.modalCategyPanelRef?.hide();
      }
    });
  }

  updateDownloadStatus(fileModel: FileInfoModel) {
    var status = !fileModel.canBeDownloaded;
    return this.fileService.updateDownloadStatus(fileModel, status).subscribe(res => {
      let index = this.fileHandler.fileList.findIndex(x => x.id == res.data.id);
      this.fileHandler.fileList[index] = res.data;
    });
  }

  onSearch() {
    if (!this.searchValue) {
      this.fileHandler.setFilteredFileList(null);
    }
    else {
      this.fileHandler.setFilteredFileList(this.searchValue);
    }
    this.page = 0;
  }

  handleFileInput(e: any) {
    this.fileToUpload = e?.target?.files[0];
  }

  onUploadSubmit(template: TemplateRef<any>, templateSuccess: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template);
    this.currentSubscription = this.fileService.uploadFile(this.fileToUpload).subscribe((event: HttpEvent<ResponseModel<FileInfoModel>>) => {
      if (event.type === HttpEventType.UploadProgress) {
        this.uploadProgress = event.total ? Math.round(event.loaded / event.total * 100) : 0;
      }
      if (event.type === HttpEventType.Response) {
        if (event.body?.isSuccessful) {
          this.uploadedFile = event.body.data;
          this.modalRef?.onHidden?.emit(() => this.uploadedFile = null);
        }
        else
          alert(event.body?.errors);
      }
    },
      (x: any) => {
        alert(x);
    });
  }

  onDownloadSubmit(file: FileInfoModel, template: TemplateRef<any>) {
    this.uploadProgress = 0;
    this.modalRef = this.modalService.show(template);
    this.currentSubscription = this.fileService.downloadFile(file).subscribe((event: HttpEvent<Blob>) => {
      if (event.type === HttpEventType.UploadProgress) {
        this.uploadProgress = event.total ? Math.round(event.loaded / event.total * 100) : 0;
      }
      if (event.type === HttpEventType.Response) {
        this.uploadProgress = 0;
        this.modalRef?.hide();
        let downloadedFile = new Blob([event.body as Blob], { type: file.contentType });
        this.fileService.createFileLinkWithClick(file.title, downloadedFile);
      }
    },
      (x: any) => {
        alert(x);
      });
  }

  onDeleteSubmit() {
    this.fileService.deleteFile(this.fileHandler.selectedFile?.id as number).subscribe(
      res => {
      if (res.isSuccessful) {
        this.router.navigateByUrl('/files').then(() => {
          this.fileHandler.setSelectedFile(null);
          this.modalRef?.hide();
          this.getFiles();
        });
      }
      else {
        this.modalRef?.hide();
        console.error(res);
      }
    });
  }

  openModal(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template);
  }

  openModalSelectCategory(template: TemplateRef<any>) {
    this.modalCategyPanelRef = this.modalService.show(template);
  }

  changeSelectedFileActive(file: FileInfoModel | null) {
    if (file != null)
      this.fileHandler.setSelectedFile(file);
    else
      this.fileHandler.setSelectedFile(null);
  }

  changeSearchStatus() {
    this.searchActive = !this.searchActive;
  }

  changeUploadStatus() {
    this.uploadActive = !this.uploadActive;
  }

  defaultOperationStatuses() {
    this.uploadActive = false;
    this.searchActive = false;
  }

  changeCurrentPage(e: any) {
    this.page = e;
  }

  openModalCategoryPanel(template: TemplateRef<any>) {
    this.modalCategyPanelRef = this.modalService.show(template);
  }

  deleteUploadedFile() {
    this.fileService.deleteFile(this.uploadedFile?.id as number).subscribe(() => {
      this.uploadProgress = 0;
      this.modalRef?.hide();
      this.getFiles();
    })
  }
  saveUploadedFile() {
    this.uploadProgress = 0;
    this.modalRef?.hide();
    this.uploadedFile = null;
    this.getFiles();
    return;
  }
  cancelUploading() {
    this.currentSubscription.unsubscribe();
    this.uploadProgress = 0;
    this.modalRef?.hide();
    this.uploadedFile = null;
    return;
  }
}
