USE [master]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'SPPID_Staging_{context}')
	DROP DATABASE [SPPID_Staging_{context}]
GO

CREATE DATABASE [SPPID_Staging_{context}] 
GO

USE [SPPID_Staging_{context}]
GO
