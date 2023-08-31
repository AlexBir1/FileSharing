

create trigger files_after_delete on files 
for delete as
begin
	declare @oldCategory int; 

	select @oldCategory = deleted.category_id from deleted;

	update Categories set ElementCount = ElementCount - 1 where Id = @oldCategory;
		 
end