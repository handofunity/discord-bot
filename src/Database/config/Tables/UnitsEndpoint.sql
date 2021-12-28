CREATE TABLE [config].[UnitsEndpoint]
(
	[UnitsEndpointID]			INT	NOT NULL IDENTITY(1, 1),
	[BaseAddress]				VARCHAR(256)	NOT NULL,
	[Secret]					VARCHAR(128)	NOT NULL,
	[ConnectToRestApi]			BIT	NOT NULL,
	[ConnectToNotificationsHub]	BIT	NOT NULL,
	CONSTRAINT [PK_UnitsEndpoint] PRIMARY KEY CLUSTERED ([UnitsEndpointID] ASC),
	CONSTRAINT [UQ_BaseAddress] UNIQUE ([BaseAddress])
)
