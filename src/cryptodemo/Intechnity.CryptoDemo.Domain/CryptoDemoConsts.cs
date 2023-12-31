﻿namespace Intechnity.CryptoDemo.Domain;

public static class CryptoDemoConsts
{
    public static readonly string BLOCKCHAIN_ID = "CryptoDemoDEV";

    public static readonly string BLOCKCHAIN_VERSION = "0";

    public static readonly int MINTING_DIFFICULTY = 1;

    public static readonly int COINBASE_TRANSACTION_AMOUNT = 50;

    public static readonly int MAX_BLOCKS_WITHOUT_BALANCE_STAKING = 50;

    public static readonly int SYNC_STATUS_BLOCKS_MARGIN = 3;

    public static readonly TimeSpan MAX_WAIT_TO_LOAD_BLOCKCHAIN = new TimeSpan(0, 1, 0);

    public static readonly TimeSpan MIN_BLOCK_TIMESTAMP_DIFF = new TimeSpan(0, 1, 0);

    public static readonly TimeSpan MAX_CLOCK_DIFF = new TimeSpan(0, 2, 0);

    public static readonly string GENESIS_BLOCK_MINTER_ADDRESS = "30818902818100CD225682390723590149ACE3B1F2C4F0E28AE4E0694EC943E57D878A4F979A41CE58B56A67C1EB1D2E33A54E72AACA0B0A71101612853E22E4322F7DD58D40F3E84DD3FA06088E6AD2240BA7256A5E9AE0403364836C35894DD94658B4F275BFDEA69DA45F839BFB2159D2845DD350342D74BA7A440C2E377BEAAC3D39A6395D0203010001"; // todo replace for release. Private key: 3082025D02010002818100CD225682390723590149ACE3B1F2C4F0E28AE4E0694EC943E57D878A4F979A41CE58B56A67C1EB1D2E33A54E72AACA0B0A71101612853E22E4322F7DD58D40F3E84DD3FA06088E6AD2240BA7256A5E9AE0403364836C35894DD94658B4F275BFDEA69DA45F839BFB2159D2845DD350342D74BA7A440C2E377BEAAC3D39A6395D02030100010281804550A20CB4BA6DBE488A1DAA7ED9BAA46ED86208566D31E3086BC75DFC110D25C954FE502B29428A04AF9CDF0A2E1DC16750D70FAE4869BB9E823ABBF96A1694F97BB322DC43169B11053A4CC79744C10C0A434B7A8415368ECC732D2CF07ED145B6F2B5827037E96899163374F647CDC040BCD1B8605A42C266D895B77B403D024100E804F1BE8E5FC22B539F873F59B702DA0C103E923452A0F22369228A39A799BAACC27F0E91123FA3E105BC56B6EA1F269A690302E310990F1767CFAD4BC1A903024100E2560833739D2884FF8F78F8EC61735AFF684020A19F1A81F6DB41B791692C4BFBB4BDB1B016C56A60A227A0BE410160E643050A95741A047D99EF927BB9961F024100916F47F9225573E8A4AA42A4BB1FB471E94DE56ACFD15B816C20E2BDB216148EA6EBE3A8C5D6A27D9EF7716F098907ADB2EC502EE715E85B4558951D137778F102400D9453D1A2721F683B5D04490B059DC22BE8B9503BE22BD8F8529752C82AA339BBD4503D44EF58D0D51365854364EB0C41446C1D027280CD1C2C0C2FA1B4B4C90241009FD8CCEE7FE99E0396C5D621CC0A432E71AC6B05AD2B76E7D6E6B781E499D96D6E6965B9A0D494977CEEF0D2726F465BEEA490210DA62AF5CE31B259562EABCD todo replace when ready to publish

    public static readonly DateTimeOffset GENESIS_BLOCK_TIMESTAMP = new DateTimeOffset(2022, 9, 27, 0, 0, 0, TimeSpan.FromHours(0));
}