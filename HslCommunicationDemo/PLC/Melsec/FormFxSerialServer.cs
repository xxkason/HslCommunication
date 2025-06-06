﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using HslCommunication;
using HslCommunication.ModBus;
using System.Threading;
using System.Xml.Linq;
using HslCommunication.Profinet.Melsec;
using HslCommunicationDemo.DemoControl;

namespace HslCommunicationDemo
{
	public partial class FormFxSerialServer : HslFormContent
	{
		public FormFxSerialServer( )
		{
			InitializeComponent( );
			fxSerialServer = new MelsecFxSerialServer( );                       // 实例化对象
		}

		private void FormSiemens_Load( object sender, EventArgs e )
		{
			if(Program.Language == 2)
			{
				Text = "FxSerial Virtual Server [data support, bool: x,y,m   word: x,y,m,d]";
				label3.Text = "port:";
				button1.Text = "Start Server";
				button11.Text = "Close Server";
			}

			addressExampleControl = new AddressExampleControl( );
			addressExampleControl.SetAddressExample( HslCommunicationDemo.PLC.Melsec.Helper.GetFxSerialAddress( ) );
			userControlReadWriteServer1.AddSpecialFunctionTab( addressExampleControl, false, DeviceAddressExample.GetTitle( ) );

			codeExampleControl = new CodeExampleControl( );
			userControlReadWriteServer1.AddSpecialFunctionTab( codeExampleControl, false, CodeExampleControl.GetTitle( ) );
			userControlReadWriteServer1.SetEnable( false );
		}

		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{
			fxSerialServer?.ServerClose( );
		}

		#region Server Start

		private MelsecFxSerialServer fxSerialServer;
		private AddressExampleControl addressExampleControl;
		private CodeExampleControl codeExampleControl;

		private void button1_Click( object sender, EventArgs e )
		{
			if (!int.TryParse( textBox_port.Text, out int port ))
			{
				DemoUtils.ShowMessage( DemoUtils.PortInputWrong );
				return;
			}

			try
			{
				fxSerialServer.IsNewVersion = checkBox_newVersion.Checked;
				fxSerialServer.ActiveTimeSpan = TimeSpan.FromHours( 1 );
				//mcNetServer.OnDataReceived += MelsecMcServer_OnDataReceived;
				fxSerialServer.ServerStart( port );
				userControlReadWriteServer1.SetReadWriteServer( fxSerialServer, "D100" );

				button1.Enabled = false;
				userControlReadWriteServer1.SetEnable( true );
				button11.Enabled = true;


				// 设置示例的代码
				codeExampleControl.SetCodeText( "server", "", fxSerialServer, nameof( fxSerialServer.IsNewVersion ) );
			}
			catch (Exception ex)
			{
				DemoUtils.ShowMessage( ex.Message );
			}
		}

		private void button5_Click( object sender, EventArgs e )
		{
			if (fxSerialServer != null)
			{
				fxSerialServer.StartSerialSlave( textBox_serial.Text );
				button5.Enabled = false;

				// 设置示例的代码
				codeExampleControl.SetCodeText( "server", textBox_serial.Text, fxSerialServer, nameof( fxSerialServer.IsNewVersion ) );
			}
		}
		private void button11_Click( object sender, EventArgs e )
		{
			// 停止服务
			fxSerialServer?.CloseSerialSlave( );
			fxSerialServer?.ServerClose( );
			userControlReadWriteServer1.Close( );
			button5.Enabled = true;
			button1.Enabled = true;
			button11.Enabled = false;
		}

		private void MelsecMcServer_OnDataReceived( object sender,  object source, byte[] receive )
		{
			// 我们可以捕获到接收到的客户端的modbus报文
			// 如果是TCP接收的
			if (source is HslCommunication.Core.Net.AppSession session)
			{
				// 获取当前客户的IP地址
				string ip = session.IpAddress;
			}

			// 如果是串口接收的
			if (source is System.IO.Ports.SerialPort serialPort)
			{
				// 获取当前的串口的名称
				string portName = serialPort.PortName;
			}
		}

		#endregion


		public override void SaveXmlParameter( XElement element )
		{
			element.SetAttributeValue( DemoDeviceList.XmlPort,       textBox_port.Text );
			element.SetAttributeValue( DemoDeviceList.XmlCom,        textBox_serial.Text );
			element.SetAttributeValue( "IsNewVersion", checkBox_newVersion.Checked );
			this.userControlReadWriteServer1.GetDataTable( element );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			textBox_port.Text             = element.Attribute( DemoDeviceList.XmlPort ).Value;
			checkBox_newVersion.Checked = GetXmlValue( element, "IsNewVersion", false, bool.Parse );
			textBox_serial.Text = GetXmlValue( element, DemoDeviceList.XmlCom, textBox_serial.Text, m => m );
			this.userControlReadWriteServer1.LoadDataTable( element );
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}

	}
}
