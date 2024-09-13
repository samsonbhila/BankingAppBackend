using BankingAppBackend.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AutoMapper;

namespace BankingAppBackend.Profiles
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<RegisterNewAccountModel, Account>().ForMember(dest => dest.CurrentAccountBalance, opt => opt.MapFrom(src => src.InitialDeposit)); ;
            CreateMap<UpdateAccountModel, Account>();
            CreateMap<Account, GetAccountModel>();
            CreateMap<TransactionRequestDto, Transaction>();
        }
    }
}
