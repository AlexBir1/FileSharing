import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AccountModel } from '../Entity/AccountModel';

import { AccountService } from '../services/account.service';
import { SettingsService } from '../services/settings.service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {
  currentAccount!: AccountModel;
  isExpanded = false;
  public accountService: AccountService;
  router: Router;
  httpClient: HttpClient;
  baseUrl: string;

  constructor(accountService: AccountService, router: Router, http: HttpClient, @Inject('BASE_URL') url: string, private settingService: SettingsService) {
    this.accountService = accountService;
    this.router = router;
    this.httpClient = http;
    this.baseUrl = url;
  }
  ngOnInit() {
    this.setDefaultNavbar();
    this.accountService.currentAccount$
      .subscribe(x => this.currentAccount = x as AccountModel);
  }

  SignOut() {
    this.setDefaultNavbar();
    this.accountService.SignOut();
    this.settingService.refreshSettings();
    this.router.navigateByUrl('/').then(() => window.location.reload());
  }

  setDefaultNavbar() {
    var elem = $('#navbarSide');
    this.isExpanded = false;
    if (elem.hasClass('navbar-side-full'))
      this.showHalfNavbar();
  }

  toggle() {
    this.isExpanded = !this.isExpanded;

    if (this.isExpanded === true) {
      this.showFullNavbar();
    }
    else if (this.isExpanded === false) {
      this.showHalfNavbar();
    }
  }

  showHalfNavbar() {
    var elem = $('#navbarSide');
    elem.removeClass('navbar-side-full');
    elem.addClass('navbar-side-half');

    var tips = $('.link-tip-full');
    tips.removeClass('link-tip-full');
    tips.addClass('link-tip-full-hidden');
  }

  showFullNavbar() {
    var elem = $('#navbarSide');
    elem.removeClass('navbar-side-half');
    elem.addClass('navbar-side-full');

    var tips = $('.link-tip-full-hidden');
    tips.removeClass('link-tip-full-hidden');
    tips.addClass('link-tip-full');
  }

}
