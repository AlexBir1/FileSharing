import { CategoryModel } from "../Entity/CategoryModel";
import { FileInfoModel } from "../Entity/FileInfoModel";
import { FilesService } from "../services/files.service";


export class FilesHandler
{
  fileList: FileInfoModel[] = [];
  filteredFileList: FileInfoModel[] = [];
  categoryFileList: FileInfoModel[] = [];
  selectedFile: FileInfoModel | null = null;

  constructor(private filesService: FilesService) { }

  refreshFileLists() {
    this.categoryFileList = [];
    this.filteredFileList = [];
    return this.filesService.getFiles().subscribe(x => {
      if (x.isSuccessful)
        this.fileList = x.data;
    })
  }

  setFileList()
  {
    return this.filesService.getFiles().subscribe(x => {
      if (x.isSuccessful) {
        this.fileList = x.data;
      }
    });
  }

  setFilteredFileList(fileTitle: string | null)
  {
    this.filteredFileList = [];
    if (fileTitle === null)
      return;
    else if (this.categoryFileList.length > 0) {
      return this.categoryFileList.forEach(x => {
        if (x.title.toLowerCase().includes(fileTitle.toLowerCase()))
          this.filteredFileList.push(x);
      })
    }
    else {
      return this.fileList.forEach(x => {
        if (x.title.toLowerCase().includes(fileTitle.toLowerCase()))
          this.filteredFileList.push(x);
      })
    }
  }

  setCategoryFileList(selectedCategory: CategoryModel | null) {
    this.categoryFileList = [];
    if (selectedCategory === null)
      return;
    this.fileList.forEach(x => {
      if (x.category_Id === selectedCategory.id)
        this.categoryFileList.push(x);
    })
  }

  setSelectedFile(file: FileInfoModel | null) {
    this.selectedFile = null;
    if (file === null) return;
    this.selectedFile = file;
  }
}
