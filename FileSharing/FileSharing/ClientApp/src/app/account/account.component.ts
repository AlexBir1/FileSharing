import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { AccountModel } from '../Entity/AccountModel';
import { ResponseModel } from '../Entity/ResponseModel';
import { UpdateAccountModel } from '../Entity/UpdateAccountModel';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { AccountHandler } from '../Handlers/AccountHandler';
import { AccountInfoModel } from '../Entity/AccountInfoModel';

import { AccountService } from '../services/account.service';

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.css']
})
export class AccountComponent implements OnInit {
  
  updateActive: boolean = false;
  showAccountList: boolean = false;
  showCurrentAccount: boolean = true;
  searchAccountActive: boolean = false;
  searchValue: string = '';
  updateAccountForm!: FormGroup;
  updateAccountModel!: UpdateAccountModel;
  ErrorList: string[] = [];
  modalRef?: BsModalRef;
  modalService: BsModalService;
  showSelectedAccounts: boolean = false;
  showSelectedAccount: boolean = false;
  accountHandler!: AccountHandler;
  page: number = 0;
  additionalInfo!: AccountInfoModel;
  availableRoles: string[] = [];

  constructor(private accountService: AccountService, private formBuilder: FormBuilder, private router: Router, private ModalService: BsModalService)
  {
    this.modalService = ModalService;
  }

  ngOnInit() {
    this.accountService.currentAccount$.subscribe(x =>
    {
      this.accountHandler = new AccountHandler();
      this.accountHandler.setCurrentAccount(x as AccountModel);
    });
  }

  getAvailableRoles() {
    this.accountService.getAvailableRoles().subscribe(x => {
      if (x.isSuccessful) {
        this.availableRoles = x.data;
      }
      else
        this.availableRoles = [];
    })
  }

  changeUpdateActive() {
    this.updateActive = !this.updateActive;
    if (this.updateActive === true)
      this.initUpdateAccountForm();
  }

  initUpdateAccountForm() {
    this.updateAccountForm = this.formBuilder.group({
      id: ['', Validators.required],
      username: ['', Validators.required],
      email: ['', Validators.required],

    });

    if (this.accountHandler.selectedAccount) {
      this.updateAccountForm.controls['id'].setValue(this.accountHandler.selectedAccount.id);
      this.updateAccountForm.controls['username'].setValue(this.accountHandler.selectedAccount.username);
      this.updateAccountForm.controls['email'].setValue(this.accountHandler.selectedAccount.email);
    }
    else {
      this.updateAccountForm.controls['id'].setValue(this.accountHandler.currentAccount.id);
      this.updateAccountForm.controls['username'].setValue(this.accountHandler.currentAccount.username);
      this.updateAccountForm.controls['email'].setValue(this.accountHandler.currentAccount.email);
    }

  }

  getAccounts() {
    this.initAccountList();
  }

  changeShowAccountList() {
    this.initShowStatuses();
    this.showAccountList = true;
    this.getAccounts();
  }

  changeShowCurrentAccount() {
    this.initShowStatuses();
    this.showCurrentAccount = true;
  }

  changeShowSelectedAccount(item: AccountModel | null) {
    this.showSelectedAccount = !this.showSelectedAccount;
    if (this.showSelectedAccount) {
      this.accountHandler.setSelectedAccount(item as AccountModel);
    }
    else
      this.accountHandler.setSelectedAccount(null);
  }


  initAccountList() {
    this.accountService.getAccounts().subscribe(x =>
    {
      if (x.isSuccessful) {
        this.accountHandler.setAccountArray(x.data);
      }
      else {
        console.error(x.errors);
      }
    });
  }

  initShowStatuses() {
    this.showAccountList = false;
    this.showCurrentAccount = false;
    this.showSelectedAccount = false;
  }




  validateFields(form: FormGroup) {
    Object.keys(form.controls).forEach(i => {
      const control = form.get(i);
      if (control instanceof FormControl)
        control.markAsDirty({ onlySelf: true });
      if (control instanceof FormGroup)
        this.validateFields(form);
    })
  }

  updateAccount() {
    if (this.updateAccountForm.valid)
    {
      this.updateAccountModel = this.updateAccountForm.value;
      this.accountService.updateAccount(this.updateAccountModel).subscribe((x: ResponseModel<AccountModel>) => {
        if (x.isSuccessful === true) {
          this.modalRef?.hide();
          this.accountService.SignOut();
          this.router.navigateByUrl('/auth');
        }
        else {
          x.errors.forEach((i) => {
            this.modalRef?.hide();
            this.ErrorList.push(i);
          });
        }
      }, (error: HttpErrorResponse) => this.ErrorList = Object.values(error.error.errors));
    }
    else
      this.validateFields(this.updateAccountForm);
  }

  deleteAccount(id: string) {
    this.accountService.deleteAccount(id).subscribe(x => {
      if (x.isSuccessful) {

      }
    });
  }

  openModal(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template);
  }

  openRolesModal(template: TemplateRef<any>) {
    this.getAvailableRoles();
    this.modalRef = this.modalService.show(template);
  }

  openModalWithAccountAdditionalInfo(template: TemplateRef<any>, accountId: string) {
    this.accountService.getAdditionalInfo(accountId).subscribe(x => {
      if (x.isSuccessful) {
        this.additionalInfo = x.data;
        this.modalRef = this.modalService.show(template);
      }
    });
  }

  convertFileSize(sourceSize: number): string {
    var result = '';
    var typeOfSize = ' Bytes';
    if (sourceSize > 1000) { // kb
      if (sourceSize > 1000000) { // mb
        if (sourceSize > 1000000000) { // gb
          result = (sourceSize / 1000000000).toFixed(2);
          typeOfSize = ' Gb';
          return String(result + typeOfSize);
        }
        result = (sourceSize / 1000000).toFixed(2);
        typeOfSize = ' Mb';
        return String(result + typeOfSize);
      }
      result = (sourceSize / 1000).toFixed(2);
      typeOfSize = ' Kb';
      return String(result + typeOfSize);
    }
    result = sourceSize.toString();
    return String(result + typeOfSize);
  }

  changeCurrentPage(e: any) {
    this.page = e;
  }

  changeSearchAccountStatus() {
    this.searchAccountActive = !this.searchAccountActive;
    if (this.searchAccountActive === false)
      this.showSelectedAccounts = false;
  }

  changeShowSelectedAccounts() {
    this.showSelectedAccounts = !this.showSelectedAccounts;
    if (this.showSelectedAccounts === true) {
      this.accountHandler.setAccountArrayFiltered(this.searchValue);
      this.page = 0;
    }
    this.page = 0;
  }

  selectRole(role: string) {
    var accountId = this.accountHandler.selectedAccount?.id;
    return this.accountService.setAccountRole(accountId as string, role).subscribe(x => {
      if (x.isSuccessful) {
        var aid = this.accountHandler.accountList.findIndex(t => t.id == x.data.id);
        this.accountHandler.accountList[aid] = x.data;
        this.accountHandler.selectedAccountRole = role;
        this.modalRef?.hide();
      }
    });
  }
}
