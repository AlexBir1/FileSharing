

create trigger files_after_insert on files 
for insert as
begin
	declare @newCategory int; 

	select @newCategory = inserted.category_id from inserted;

	update Categories set ElementCount = ElementCount + 1 where Id = @newCategory;
		 
end