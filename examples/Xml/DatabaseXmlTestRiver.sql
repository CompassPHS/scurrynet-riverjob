/****** Object:  StoredProcedure [dbo].[river_GetXmlTestData]    Script Date: 9/26/2014 12:49:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[river_GetXmlTestData]
	 @rows int = 10000
	,@children int = 5
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Create tmp table
	create table #tmpData (
		 [id] int
		,[key] bigint
		,[code] nvarchar(255)
		,[desc] nvarchar(255)
		,[longDesc] nvarchar(1024)
		,[groupId] bigint
		,[groupCode] nvarchar(255)
		,[groupDesc] nvarchar(255)
		,[amount] bigint
		,[total] bigint
		,[allowed] bigint
		,[isActive] bit
		,[createDate] datetime
	);

	-- insert random test data into tmp table
	declare @idx int
	set @idx = 0

	while @idx < @rows
	begin
		declare @inner int
		set @inner = 0

		while @inner < @children
		begin
			insert into #tmpData
			values (
				 @idx
				,abs(cast(cast(newid() as varbinary) as bigint))
				,convert(varchar(255), newid())
				,convert(varchar(255), newid())
				,convert(varchar(1024), newid())
				,abs(cast(cast(newid() as varbinary) as bigint))
				,convert(varchar(255), newid())
				,convert(varchar(255), newid())
				,abs(cast(cast(newid() as binary(8)) as bigint))
				,@inner + 1
				,abs(cast(cast(newid() as binary(8)) as bigint))
				,1
				,getdate()
			)

			set @inner = @inner + 1
		end

		set @idx = @idx + 1
	end

	-- select data out of tmp table
	select
		 id as "_id"
		,(select top 1 id, [allowed]
		  from #tmpData
		  where id = d.id
		  for xml path (''), type) as [name.]
		,(select [key] as "key"
		  from #tmpData
		  where id = d.id
		  for xml path (''), type) as [keys]
		,(select [code] as "code"
		  from #tmpData
		  where id = d.id
		  for xml path (''), type) as codes
		,(select [desc] as "desc"
		  from #tmpData
		  where id = d.id
		  for xml path (''), type) as descs
		 ,(select [longDesc] as "longDesc"
		  from #tmpData
		  where id = d.id
		  for xml path (''), type) as longDescs
		,(select [groupId] as "_id"
		  ,[groupCode] as "code"
		  ,[groupDesc] as "desc"
		  from #tmpData
		  where id = d.id
		  for xml path ('group.'), type) as groups
		,[amount] as "amount"
		,(select [total] as "total"
		  from #tmpData
		  where id = d.id
		  for xml path (''), type) as totals
		,[allowed] as "allowed"
		,[isActive] as "isActive"
		,[createDate] as "createDate"
	from #tmpData d
	order by id asc
	for xml path ('type'), root ('index');

	drop table #tmpData
END