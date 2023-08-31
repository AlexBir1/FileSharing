create trigger files_after_update on files 
for update as
begin
	declare @oldCategory int; 
	declare @newCategory int; 

	select @oldCategory = deleted.category_id from deleted;
	select @newCategory = inserted.category_id from inserted;

	if @oldCategory != @newCategory
		update Categories set ElementCount = ElementCount + 1 where Id = @newCategory;
		update Categories set ElementCount = ElementCount - 1 where Id = @oldCategory;
		 
end
