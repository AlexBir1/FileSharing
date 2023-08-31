import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { AccountService } from '../account.service';
import { CategoryService } from '../category.service';
import { AccountModel } from '../Entity/AccountModel';
import { CategoryModel } from '../Entity/CategoryModel';

@Component({
  selector: 'app-category',
  templateUrl: './category.component.html',
  styleUrls: ['./category.component.css']
})
export class CategoryComponent implements OnInit {
  categoryList: CategoryModel[] = [];
  selectedCategory!: CategoryModel | null;
  showSelectedCategory: boolean = false;
  updateCategoryActive: boolean = false;
  newCategoryTitle: string = '';
  createCategoryActive: boolean = false;
  currentAccount!: AccountModel;
  public page: number = 0;

  public modalRef!: BsModalRef;
  public updateCategoryForm!: FormGroup;

  constructor(private categoryService: CategoryService, private modalService: BsModalService, private formBuilder: FormBuilder, private accountService: AccountService) { }

  ngOnInit()
  {
    this.accountService.currentAccount$.subscribe(x => {
      this.currentAccount = x as AccountModel;
      this.getCategories();
    });
  }

  getCategories() {
    this.categoryService.getCategories().subscribe(x => {
      if (x.isSuccessful) {
        this.categoryList = x.data;
      }
    });
  }

  addCategory(title: string) {
    var category = new CategoryModel();
    category.title = title;
    return this.categoryService.createCategory(category).subscribe(x => {
      if (x.isSuccessful) {
        this.categoryList.push(x.data);
      }
    });
  }

  updateCategory() {
    var model: CategoryModel = this.updateCategoryForm.value;
    this.categoryService.updateCategory(model).subscribe(x => {
      if (x.isSuccessful) {

      }
      else {

      }
    });
  }

  changeCreateCategoryStatus() {
    this.createCategoryActive = !this.createCategoryActive;
  }

  changeCurrentPage(e: any) {
    this.page = e;
  }

  changeUpdateCategoryActive()
  {
    this.updateCategoryActive = !this.updateCategoryActive;
    if (this.updateCategoryActive)
      this.initUpdateCategoryForm();
  }

  initUpdateCategoryForm()
  {
    this.updateCategoryForm = this.formBuilder.group({
      id: ['', Validators.required],
      title: ['', Validators.required],
    });

    this.updateCategoryForm.controls['title'].setValue(this.selectedCategory?.title);
  }

  changeShowSelectedCategory(item: CategoryModel | null)
  {
    this.showSelectedCategory = !this.showSelectedCategory;
    if (this.showSelectedCategory)
      this.selectedCategory = item;
    else
      this.selectedCategory = null;
  }

  openModal(template: TemplateRef<any>)
  {
    this.modalRef = this.modalService.show(template);
  }
}
