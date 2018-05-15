INSERT INTO [config].[Message]
(
	[Name],
	[Description],
	[Content]
)
VALUES
(
	'FirstServerJoinWelcome',
	'The welcome message sent to users who join the server for the first time ever.',
	'Welcome to the Hand of Unity Discord. ' + 
		'As default, vision/use of our text and voice channels is granted to people with guest permissions only (hence why the Discord seems empty). ' +
		'If you would like to access our actual guild areas to participate then please contact Narys or type in the public lobby.'
);