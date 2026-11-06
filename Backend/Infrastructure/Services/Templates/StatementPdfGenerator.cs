using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.DataTransferObjects.Templates;
using Application.Repositories.ReadOnly;
using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services.Templates;

public class StatementPdfGenerator : IStatementPdfGenerator
{
        public const string DefaultLogoBase64 = "iVBORw0KGgoAAAANSUhEUgAAAJYAAAA4CAYAAAAfFwF8AAAAAXNSR0IArs4c6QAAAAlwSFlzAAAOxAAADsQBlSsOGwAAABl0RVh0U29mdHdhcmUATWljcm9zb2Z0IE9mZmljZX/tNXEAACKXSURBVHhe7X0JfFTVvf85595ZsxOyEYgYwh5QiwQJEMQBI0ZQJChW1Orrs311qdpWebWGkr5ate3rv49aP2prtaaKEIVWR4gSlkgmEEiESggECBCyT5KZZLa73//v3CQQkplkQgLE2vP5DAm55571e37ntw+7fv16dLlLTk4Ohj5wbm6ucrn7+nf7I2MFWK8hy7TFzs2oqpcYGJIMH3WoQ2ORykTFmFofjy44QdvavHkzNk2fH9G08K6Y60LF2kar1TvUPi7X+3AImAdf3pP0fgUbLUkIh4YhxoCUEJ5VfG4HEukBmZbEeiqeM9bkbrC6gh1HZORqXUKyPRIhH2MyRg1qjR3QSQKK8thseW7a39SpU0lUcnKYo8FhGmxb/Y3XxznwqjuiPLm5wc8rUHssinLovXWmJ5Eu5G7AlASfTqqCtbnT/3ctgvb3oBZEwYRVRPmLyG9lP+Isz3dUVlYqhqhkyaQzzH7bYVrelJa979Rt04qBggXVXrCbN5R6O5WFM4826ZO255Rc19DOZioMnoQwxm5ORR7GYFZ5iUN6OHgqIo1OtTXtN0Jha4flC0RQ3e8fjDpRbs1v6q//Cp8r/sMy9mdt3pBrCOF5rKr0EAdRCDbokPStxNaNr+XkfETXLDk52fDyTuHhytaw2wgSBIxhn7q2LYgGA1ZRkYm8YuMadaylQVCI0OgitS6v0PDsLeSkfKLwzGDaZs2NtvZQY+Z747AyW1HVUBUmjLGiygrWt/FMvICMndjC+k6CpgDWEGCsG29+elMwg3yCb96m0vYlt7JoE63C26yumy3ZnxYJ5Cc8Qn+45rOqrTE3Ze85eCu7Nzd3I6UEV6XsVC1pZ9uUBZzE3N/qQ5NaarAJMUaiEjrnziFp6Cf60O4BOkQcc7BengJ7+XC4QXU+u9G50ydb/l7+QsL2DRs6qUrv4mIlnYuXr612MEu8sklrG1NsnT+vXZ31eZNBRiKgcL3wZY+DiAUFxzR0qPPbBHMIYL2rrSCxOtBKa0PBMDRJ1mHG9fZBXMoySz6P1iu2W/WFtoFep89Z+k/lKzE7f7vFfn+DXQqhO2wysXJji2D8xzEpReB5c7SJDYk087Psbnl+h6SPhpMMGBO6l9xPPxj5CBtTZRdXrl2ZtbPcZm2hlcoL8x1PZGX9768dYaFHRfTTDqJ7dNxnymZjWvaBF5OMuyrz8+qCGfRw1ElLSzO8aIu4v9KuPtnqY2eqhMWIwLwU2BwJoN8PcVY1xBGEGMbcIRFzR5O4xqRHt123ruGDMZGZn+7LnVMAILhol69LimlKimrIaXZJ7+l4z3R4eJ+bQ4naWvZ7ETBIwQgWm9ATrRW73S7cPmnMO80eV1lFk3uKiSXL7R48W0GYDJlyaePpBBaMjRGRPvKkA9+KFHHJKBM5Uqdf9LeVk/Wbw1sKqvvbBw1Yubl5HPw4GN6jZjL8/lQi2tH9p6ysrITvvK/M8qniwiYX+rZbNY5BKsCQ3p59igrzk5FHJIue3s5lPBCOPuquctpqbXkxO/vFZ8/hiGNM6INOne7JUF+7++kacbv3xtXlK43S/jdvYb8YDiqWkzOVWbZnRma1qFTda8s/2T0GQ/qakEe21q91Cehxt8BGajiR6ZEK9mam9QAachd2CFBoCY8+144f8wjS7dOfLf7D4rFpr0Y7SylCtcJX5nnMCO17NAbtAz6O3PjT4kOnHWhtmwdNp1du4L4xfawAjM8PrrS0lC561W0sfBIRao3M+MxarfsptLccwAW8crDz8Lt1F/54vhkYHmFwmw/PaPexv9p0XJl782TLyykthSWBwKUBK5hitVobVoWjT3JyVhdM/XHrDgfHPdvixvNlzOiRP3YBxuKFK8PLq7elZ2XtsFmtHd39lOfnNz6euXr9b9u9IacF40q33hzqlsRsHVaz/y7iUxu3o3149n2195v5PTfEkP3fmzbNuX4QEiXduLV7ylInFIQ8FMHiiJt00ovdfcP4dXFPND3X5tM/J6msDqlwpvq51oNZm8750xNOUBvHXusRlV843OFR35ub/b/rHpjm7M1LUul4uRH97X3Toii3QF4SZBLidw2D6hyhaGfRwfRxlt84OTStlWMnawd+WAsgjLJAgH8ZsbiuXbmz8KgafWbUohcWs7t2++sqaGB1v0wpyX3hqKBpWsapnRWGH1e1qP+hYBYEwV6Uq3OzVCdH7vjpFmnzHXr0ec8BtBRsrH5g4V25f/ahsDpkuFWFTRGB37DrTBNggyawkoA2CXjNu7XoKDl7ovmdtOymBUZl/1jMnzosmprKQLwycT44yLREqVFGjrkjTBq730PSwreduJGQ8FSOsGnNsrj2ruKtGtlet24q+b8m+386vcyPZUx0SPYN4/LDfLsAxqt6s52X175b7gy7Y1rZL6CTVn8d3TnFvPFPXwqLW334Tu1KHALA/5pUv2/Uyfi3YOtfUikFHEJbARdFa1OEq5lBDS48X1LxeiZ24dOL8J7y3u8MGljdDcTVFp38yZK7Xvmf7Z6xZ53KUmDnuza5uwZlfjH2yvqEFg//UPx16SWNlbaLGFuyZ+s/l2fc9cz7HPmtU2fMVBVohQNNBBVzCJx+vQl4EJJI+YZ6gUNbeLUFkZAOoDEeEREBG0POs9fnVDM54VLDgeGM9egNoQqrR5Gc++xNBu6L7hElPzkmvcZFXpCx3hQ8qHpfK4GY7O5eYA7ABiisju0QJeJwODyBNsrcaG3hJUsZwfhOZQi3F20/FyTv5OjYkhMtSlMHx8ZrAv1lKyAcAyFodavpdUb2mchFWU87y632nt1dMrBoI7WlW6tvHGf5fYtHTXaJuqmdJ7bHCmkSj4IaXeiWtyrCZt1O0J7ec40r2loxy7L6qQMu3wYXa1ysUHDR9yj/4qM47LxiOAAax+pHA8M8GsHvGp/aozGQaJGoMd7wTBQ1toVRpd3bkprLc6Ged+qaEHZ/4z3QbDzC51mfAEtP+wRWBcPykB5LRMelScU9pbleTcA7hGFQKOPdGDba9EubrZDyrwHLzSnkUFG12tTBs3GUGgylvLQ05vjDH7gOdwi6eCRfRmBpC68gGW6qGqd8x6ZSYXdxbs6fgV05vyVDAhZtftOP5u0Y90Pbxz4XmiABKejDOMIEOaRLqGmVH1n9xOqSjRs3UnHyorKgcOOx5rS7nqyXlJedKrlVYlkDAu3kBXmfAo1+qLAwwNJrAhuI6KLPnUCUg7l5Nm1jtx1uSD3TxtwJ10QAgYPW6gIUgMnM8LKeUVsArsAbglIPLhf4Vw+KmBi3rA+lGNYOUs/DBG1joJShhPtsbBR+7l5i7Ve3RXv89pyQiuKzvkYYc1ynAHHpxVaY3+wSMs9q0m0gzQMcSNBZdh0YP9SYXncUlNpY6Hr3ZyyREVz7EYcb+EceeLnYCi80dI9+yMCijOnyyZnvv/ulvMylAtVSeuOGSogItfvU+Y++75hzC0bnr6aeS7iydGvlyYysZw4IxheqFbJaMIAAD9ffxbwC3fggFp6BaUnCsaxRvgJaOxJUC+YSZqmsKLH9N8AgAyNLBix9Pj5aKZ6fxBxJSmDPUC0x7IaC3L5wW406c28NnwraiWm8hOb7ZD0IAHQDAHusCVSMvgNjw9Vn7jUX1gQxUuSyiz44M0NDVI+OXJzUqq2RP54NQKXDigfJnoOSTJwM7lQ3nS9wWxh0jM5sVKIESZnQzrOjtPWnTI4/no3+DSg47Pjkao5kpmD09rABizY0OdlxJLoqbLunXU5RMIaF7nUSgLHnFDb5ZIuweveGnH0ARr8LmVJkPbnMkv3T5zxiU52ofl/AJEzb08FolekVCCsbgZSK6ppZ1anIil7fHRZS75AzVKI3durf/BUGgfqufnKs+k5mAnrV7NxVh0D7xmsauAtlDkK2OSDiG2ZkTXi7mFtR2y48CGqVGSprRAbEHY8OV394b3hhRTCgonXAXOPl4eB36rOGXhSNhAZoC673KJN6emqs8INbyJ6jkatX63r2OKm6Wo2JmaOvdlfFvVEkz/2yVl3qlfHtPokZFZBng+5EbBhVXisuvXXcMAPLCXqVlBhLnt2jrPTIbFJfkq4CP6sit4AX3/zz4rRbCCoOtISVhfl1b6xZk/NYFVfbojCPt7GGiZ08V5A8A/A3RJHbrmeUD1MrO5WUMSY2usqOx4COJ0C3GOkYpI6PUrZ8+asxgHtNr9dv4b+yngLp+DefGi2lwGf83CVwkyJ00nPfDd8VULfjr8HqagcvyWGCdn0NT+m3IUxUMK0ZOXpVOjdebPEo1frXdGSnF8Dn0VVZBb/ZI5/9Z538fU4m0ZrFxU+h28MyOGlqevbYSlt+La0y5Kuwu5+oSQnHY1sbt59pUx7t1Ez3olpYQR0cmXTOKS/LeXJNWW5e4M2zwbPjOTkblu48VlMqij9r04fMgqsNDk3nldNfAWUxCpF9Z+wy+rK73sRoNvlgnTJKEgIwHsBTMVisTAyT/hIMqHr2f7u+sOjIuMxnSpukhEcid20bLDYcKAoWSxw2VAXRP+ZESddbhvf3XqXN2vqXH2T/9raXHVPrO9i7ZSAOfplcQBawtRO2HHPdOAVkumEFVipoltujMze2uOU7XQJIOL2VdAAICrgWL1lx4xvn3l9uRof7WwTKu81FaKs9I7smVHA906wwd3KgRtDA1a3x7t0ASIuqLApjdejvD+KaZmfXc48ogCRIDAH7g2uIk/HJHVHc4QXdLwWxQ91VUvmCQ6mR6NAgXvnaVM1/M98xJirzz00uaS5IgQn+LS1gZRFI9PavpOlTEtHWYQUWbWx8DLv/ywb5Y7eEvwum7F7opkwgAaqFxvOIvQ204xUAngHvt+VF+eXxFsuTb/DRtmpeebwDkcmy0USN+p12PY0ydh14ODZ6WXTDvVII1/N5ZqrWIcUrCkiaqj/CQDlYBcWGKq3ZKBkqdF4I/y4XVsDexpaLktSMGJLgX9pUkaQQ4vCABaGrDNtVSNtLbLR6U2KWWL214iqPqgP+uRejTIFAGN05B1mz6Jf7P16E0NHeG5iTsw5XV79uyMtrPM/nNBYWth3MyXnt7t1HDxwR8PfPCYpFwsw1kgnmATorjaej+i8AViwSSlcg96me7bb51DB4DHP1f41SvAHTS+YnTJIrL4Fi/auDsB5EDCNjAhmd3jqBCiUcF2x7wwos2uXT8+J2PvZR42c+WV3ZVxuvDQqDfWy6vV2+D4WiF/oOUwXr/drUhZZ98sJ5uw9129noz+sROjApK6vqsMs04zinfLdRQOmgl0rmjWaGmv2IKIC6C1vNNmtjz3bjQnQt1e2ER7Jq7rsyKvwZtPxeHLvDscMMAp9ft5d/dfD0Nz9w8kEyi0Wu3/uFWlpgIS8HxaJtVtryOuZcs+Tdz4+LN/GYHdfXhgjmDqLH5xxyVklE5rtz5YKqnpOiAEpP39Esk7YXduxZ8sWaNc9tyutSctJ6Zqu1HXivvUnpWYeRKWxCqVe6p8In3ag3GcdFybxzCuEO9F6kuDDSASqIwM5KIHGyLJ5a22pYAcB695sMIn9zd4DaE5tUkwqKr8CFkv0Lz4edYtGOTVH6PWYDdwRoZyLw7L1siFRZLSMvItcda5bumxuN1vcerM22vCY945NiSSp78cTZ701Py5jzWmnRlDM96yWC4yD8/9CXaWlHJuhjzGDkTog1c+rzc2ZW59q2XtRkmImcVZBCvStAL+avqMgnk/EHa8ljStIS57ZVcZ/3J7V+04B3x3VRowpOCWFiPxI5BrOETofP07TLAqzURmtHY7zlvZIz0lyJgL+TZl/rWWQkqTrSIahLjo2+690pLZ3eBxcVSdkOnnT/oSo1PyFEnJM2//AfJo3fW5CXt+EiP/Nc0KE9hBAFTQclSbkFF4OKtlnWTE5yvOAAL1AwaPsHFpVYm10obe8p5bVJrza/3eReVJI+Rmneti7mZG5uPugxv7mlySPcBWqccRof67eAigdiAqbHC/Xdjy8LsGjjBmTcHm3i9zZ51Tv8EwmwkHBqWkWNJ+u9l6a+mptbedGogWo1pqd/+pqM/5lmNNYt9PrMM0+cTvswbT4pCDP8T0lhYWXQ3qYODjmjzGqDg8epAS1fVMuHCXZL+sQTLeJzDMNwh+2kNnVtR7lXuO1MU7tY4+UVMXUMafxdFnti8cJJdbm5G0ZsUEjgY4BVFsz1wR6TYiZz0VfN8iMCWHsC+4xRBTM+sXQqW4q08JlhVJD2HugCbG3xxC76q7NGXcQrLDiy9fbXUpCgEl1Dh7J62fqkT2bjytN9JxuzB6lhuwRRyNSxHIS2nP6uJIaucvNrP01Ldxwg5IYTY+PfPDRtWnJjf6qLUy/NcoHH5mZnvbQAvBaMgReIavhBksUMKyMmtMFLpjT4mCmdHqagHWQIOtehND3+CTrGb6k6Xd9uOTklDp3c/0Nj4StvWv36XAW7gVeqHrBJSpSJFcCI1G/JyloT/l9bGu4+YZcfg/2b0PfW6fE6qJE8vFSzbta8itwThZcXWLT1EGzeGaLnPuN5skLTNV10R2v+WqjVg26wy8otObk5oPW+OO7QZptdl55h3ySL3oWY+IwKOOgyjCdCVlz3qZi9T1GP1DXYlxxv2ONqvWl+1A5FeOyt0lJnH9mFthsVsrAw0oRPtAqGGUgaIOiASs2aElaz5nb9AJsL/NrO6+LbRervhG6mrjtnHVJbyq+EHSFGy+63vhP1QWF+ftuVAsmg+wGq7PCh6H8cQz/ZrFjaNY11gOvk/73RmCSpZDkouyM63XkCKRoI+JQqKM6snsndmHtev3TZrkI6XqBara5RSzZ/1Syke0UC/ka9jdNUQmQMVXblwfnryz69FV9wuzg/X91t25B8Yi/GHPhqUXUVxDyAvoqBKBqGaU00meyJbrdOJiRpZ7srAWo4/S7Vq6sSzi1+1f5OKye/DG4IYBkcUDfbtZjUCE5/7b494D1NP0cBh1G7wI5qF5l7TF5+afYfHRYzY/m/h6ILiwa96VfiBTgwPh4COBjdY7B4/fSoIoeGJeobN0AsAN0LVT6TNBr9rWeDlxVYtKObIzzbKuzmOzEh96qam2RPcGmbRtq9ygzwDLDAw7zes7UVkob0hTPek8QDMxmGj5VlovGQ9COBUx+lYrIUXTJ21LQttqKL+bSebYEfmDh5TPZfBKljdrNP1zmWYEP7+mxB1zy0H9SZgCCfQsI4n3GlDvFTP41a+tTtZNtFrthXAjdB9UGo9yKMWbNaBCpdVpOBvEpg3gzY2FKi0Udv3RdTtnHjhfYuO7DMvM2Znpy5seiknMGpTF9bE1AOCL2KBFPQA4nT0j9Lcdqa+0xXnPypqpY/RDCJ6SQVnYUeOrixOB2TlD9t1rIWZK3sd20X8Pltc2ZZXn69VE5oFYwZqqZ3oc569LX+jdv9NtzlKasCGyawxmlH6rlfu0Zl3n+vuSBo95mgQDEsleiBGsJce44BYsSNxFcdEyr9lR7cK0qxaGe3zI4pLKtp2Mf58F09gdE9ELjqMVyVC441mpalGNGfe6+fzZbYlD5/Zp4oHbyBYeRwWbNDdoFBDa0Fj9MikCoHYJw639DXFn5500TL4yUn+XVeAS32qYYIzSREI78HOqEDbSylXpKPRk5fV+fg1tdFWR5N5AtHLs810HwCPof1B1AxquAO16NfQjBFH4eCy06x6Nh4W55rxpglb5SeFRd4Zd1oCH68eMjwfwEbTMcdyvK48Vl/n813Brj2LJwS9TmLQytURBXvndRKc24kMVtnz/z9sdI91DM2uDLbU/jV9Buyvr+tmnv4dBufBVkr0nyKyaS5G6mUf6K8RQAXkYG60PziRSTKZNmBdvnuRCP600CvfK2eaw6JlGGXOGDYX6rdMO+93NxOSbBnuSLAoh3O0ev3VZn4Qq+b3OvPbZYGuHK8eutJO7d0dnhfs4rP8fG5iOib/iorh7/FMBK4wIBBVA2rRzi8cMMG66DjuGiEzEoz+rUyy7JlcxlZ1ejkvqXX4dR2n5LiU00QEQHOlRpIugMnuph9TUk4gBoIFMI+ZNDXONWl6qKsLbjy66GK6B/gVFihwSUEhTB8XbhReS3nzsTf9o747m7jigHLzFud46Itf3bx4jwXj8b2ARdcI4LMGl2cvNw7yWI1N158hVRW5ioZGVGf8yi8jJCWdHpqZNWwMzridcglEDy16r14pLLw5L1m9KsnnlsT9sYOe+prReqcsw5fQlioEBNpxAkQiBbLiSi+1YvHgG4Luu3y5tX83AMDTIVnvIgseSXe9Aci0cdfK6qkDbaL3aDBFzQqCuzLOiygKCP6IjFc+VP5S41/y83dFZD9uGLAokP90Xxf6WMfmbaDbuS7nae+l4QIZNbBqZayJjIfXGP/0Xsziormnropw/6BLLtuwpg4jExSAURon4+wHsrmQTIPaioqeSQGlSAqIkB59ons2Ld2O8ZsO6aM/awKXQtZFAxjR+Hpdje+3eVTRoM0CCseCFwycvNqhIOw41HkUEY23O9S/ohueyCDcrd/GzXx0ukJcJxkKS5U+qdRp26ZNx5tTXHuOpJLY+r6KVcUWJU2W/v40Us/8DZIFo+Ar+0zLpAQvRITcbRJ/k7UuKyiVKByvevo0TyrTz31METcuEMj1kAUTp8qw7YTr2zIpxJq82wwds/uGm1OTlb4dT/mPm5h1f9qdKs3Q/AIRIH7kbIoNSM6dUykMmPNmjVheXkacEdC8YKGAHyJ6aHwU0CSAgB5xkbgKgOLOnhRLW92ycctE/UVKc6CsmCX+4oCi06jNHd2UcLje3dDhPS1kCvJD9WCi84jz3UhgTLpfXzIi4qiT6VnXP8OPAurn7W3DVlTr+hmQVKyjpWQ5ORQzMLWL87ihBafbnrAXAmEYKdPuuFnn9jjwRf86gMLfPsj9NI5wPyvO3xKPWJprpFeBfSKekgWdP9kpe7GZNZ7x+J5YBMFT99Bnt8rDiwYpDAxZsnfOs6Ji72qDizmfSVEiWFHHzgrrVm6fPVOZ+nGPlatqLDn34blYBajxUouGoAmXybYXc/v2WNTF70FzP0rNN2Pf6ol06s9du9xLgxsiiOgYBSixx7LRG8B6Au1oIeABVwly+nH1lfiC2YiVxxYdFBvPxBXbNnQUHjWib9DcwD0ZoLBbMN6OTTHVuFIn4bQrp4ToflMAZza+bFag8oBFsw6XFIdl4QqdIzqExUmNFDcHVz5IW4ZwqNHSJFVlTnXZgpJ8X8RDtsorwqwgN/goiMy32p2iRavhMf1Jcc0ulY3vvCUvPqG69NL+EYblzM1hzzYUDz7npeLRbj8yodtBYbQUMZYxV3eRFqavRiA5b+oWhod4TJv4xAmcZlevSrAonN5fZmrbM375k3H7OrT4KZC47Z6TFHzQ2c4SbrLWh+yaTFBhcuOFiccakW/4nn0+abXc6gvfNA+RZdp7cBZUbsA+7WPgGelAv74V32sl2sNArV71YAF15g3OtTyYZgLr3RBSFifAYKY6xXZ2JMt8h1Ff5i6a+paMofm3IwLxwehLjXNX9JmQeI1Q+Yv2hfERZDTKU7rRdE8g138Mjsb2eGDwKB+hgI5SjviwiAvyjesXDVg0XXe+/OY8onP2Ld4BfVpGilzMa8FYflAC9y8siIjJ76s3SXdZ9IR5ygjA3ZB/7kfBto7yp8te7nk3tJzam6YXS2ris5YdzsuOjLQe/6e0zSUf/ohme9QIRluIEUp8PTRZrllcbLqpnkgvknlqgILsgPy46MtW5s86p0unknuc/JBr+UScEKFnVnf4VMTI8x4c9F/x+zasOHStujBP+y/vbhaXesUdNd0CGKSwwt2y9glv973YtyOwYbWX/vDMQ87feoDmpLUnx6L2tTAHMQwpGzFbbGNtj4OQZc2h6/LW1cVWHSRPn9+XvHkH9msPhE9Aal1et1wmpepvo0zJhsZri1ltLwNNOQBM+T1t+itY7PmF5TzOQ6ehQRxEkQqgX+7zGYca5MmjX2i8aOpoxft+P0qc0l+/sUxib3bnJiRmfyjzUI2pO7+HqSCGIMuBKb0qkogz4eiOF1KGeSi+MbFKl51YFEjZkKY5d0ap5IFET3JfaKnad4pIkPeKrX8kYzo7Y2XoGFQJmbN3FIirGt2M2mdVJF6L1AcKOBqbIjv4JUf+CSUvfg18WCbZ/EJXlLVcCPqWDGT/adBr8iV9UrM/ho1BXSHhhi7PKONYxYIIsQkEIih9se6d/nJQcqgo3PGshUBk6B9XcjPJYzzqgOLjnn3z+eVz/hJyYdH7dITQEkg2KEHXw58CoPE9knx5KNG2+D9yWkeqze+4F+obUeLO32uekifmkwHgeNatmNDLJg5btcc/3QETEsC+A3KTTRLrCDhEAkbwhTAUb1Ge8DTgaFeD4H9wEHpjvRE/fzj56PKB7KrXcK+jfhXRgSwKNUaHWnZaHTiFV6RSenJa9EIBhOrnh1rEAsGe/IN38oa906RkHOmFd/dqYgN4Oeu+XaD4EY9SrsSoMHX6KAWj6FLX04B1GUhGNBPq9NfKUwnVd2UzMJXlPS1HIx4VAzDAEcEsOg8VqyK+cr+ZvMHx+zovyFVduc3LFBLiSIL10Yg65wYsZ6/KCPDwLPffViKc3qVaRDRQ1E18Ata9sCe9YJySr3QrpaeEUMCDakjJhzlf/SjOZC9UMtW+Y0rIwZYNLtcTKjlo5p2dI9LYCZ2u9VAJtJWTpWsVPs+2N2pqnEduW686XlSL/13vYu9GQAWmGoNtvHe9TW+CnyWiMzFheJNU8IMG4JJ0zTUbkfq+yMGWHSBdq2bdyjlKdsHnKT8TFRgaCCujw7Bnz52M3vUeQlGnIcSAYwi+iz5hkz75oPySy2c/lYa8DAs/u09d5RSKki9qseiOCZC3ZoSxb44G/cvXY5UQAzXuEYUsKiZZmqMZUu9W7lHxCGTTKzXC3m5C5zluxxDmXBcbcGX86+xPFVSyz/p4vHdHkEX25l8ZogBFBo/1hmfpyeie3yk+pebJ+PfJzYWnB7KeP8V3h1RwKILOlsuLD8Ssfj9WqfwPKgYSnKWmItOXwK16r051/OFlR9tWPP09GftxW0eYUWrj2SJMmT5o6l5NOa9t5QXSI8A9bUvMKAKUOqHqYJ2XdofbiRvb/tZPOQs+ebprPwdhBEHLDrIudeoHxa4+ewQPdl3unzgJPzBnnCqXb83FOVFzs7a/dJurjDSLM6t60CWDg7HI9ZAvyKgk4r1cZvu7oECCj4Sp0IstRAbqh4QRfmT+RP0O6/3FBzIu0SLALQJ2XXBs6ZPVh7aL/2GDJk+7C90+cISQIA4RDprQkQfdYjmry/q4DQE8ksOdikHrDcigbW9vv7k7LFJvxsfAQljnQPOYdAVnJXW2u/HoT/m/GfW5om/c9/g5Un8xFEklSHKTLBPMk0udXybD8V10iztq2Yxi1VpTLh6dpSZVIJC9WCLB9VZJiFw1911BF2SLaBr2GDIQlWKG+IROQCWnyT08L2kWgpGRe3yRetfvNW+FbPLX71P5mp4BPYA0MxddmiNSGC1vbSKSoBvBwotGjSSAryQ+6bV/kAo+gxSViLYNHbzjuIxLhcifyxDMe11UqSWO4TQr1pViJFF0rwk1v7et/X18F6rFiAxLKCvhl2OzUEiFwM5VPsCC7LG0q9kUAgbFN8WZcJ5suI9pDB9k1kpio8YGdKiM407N1C2maGu8YgEVlfe0UEqkYa2FF2qAe1rSpbr0ZnlfUM94DvHILHbm0Prp/fbqZWVcuo1lX3SW/bpBVbDX2Bo73pPJRZ+BX+jn8BloO+oGoYpjkhgDcO8/t3EVV6B/w9lktD9jBLRWAAAAABJRU5ErkJggg==";

    public static byte[] GetTopRightDecorBytes()
    {
        int width = 331;
        int height = 232;
        using var bmp = new System.Drawing.Bitmap(width, height);
        using (var g = System.Drawing.Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(System.Drawing.Color.Transparent);

            // Path 1: Green curved stripe (#9FC153)
            using (var path1 = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path1.StartFigure();
                path1.AddLine(95f, 0f, 330.5f, 0f);
                path1.AddLine(330.5f, 0f, 330.5f, 232f);
                path1.AddBezier(330.5f, 232f, 280f, 164.333f, 176.6f, 26.1f, 167f, 14.5f);
                path1.AddBezier(167f, 14.5f, 157.4f, 2.9f, 115f, 0f, 95f, 0f);
                path1.CloseFigure();

                using var brush1 = new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml("#9FC153"));
                g.FillPath(brush1, path1);
            }

            // Path 2: Cyan/Blue stripe (#0093ED)
            using (var path2 = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path2.StartFigure();
                path2.AddLine(236f, 0f, 330f, 0f);
                path2.AddLine(330f, 0f, 330f, 128f);
                path2.CloseFigure();

                using var brush2 = new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml("#0093ED"));
                g.FillPath(brush2, path2);
            }

            // Path 3: Deep blue corner accent (#004C87)
            using (var path3 = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path3.StartFigure();
                path3.AddLine(288f, 0f, 330f, 0f);
                path3.AddLine(330f, 0f, 330f, 58f);
                path3.CloseFigure();

                using var brush3 = new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml("#004C87"));
                g.FillPath(brush3, path3);
            }
        }

        using var ms = new MemoryStream();
        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        return ms.ToArray();
    }
    private readonly IStatementPdfTemplateReadOnlyRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StatementPdfGenerator> _logger;

    public StatementPdfGenerator(
        IStatementPdfTemplateReadOnlyRepository repository,
        IConfiguration configuration,
        ILogger<StatementPdfGenerator> logger)
    {
        _repository = repository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<byte[]> GeneratePdfAsync(PeriodicEnergyStatementEntity statement, StatementPdfTemplateEntity? template = null, CancellationToken cancellationToken = default)
    {
        if (template == null)
        {
            if (statement.StatementPdfTemplateId.HasValue && statement.StatementPdfTemplateId.Value > 0)
            {
                template = await _repository.GetByIdAsync(statement.StatementPdfTemplateId.Value, cancellationToken);
            }

            if (template == null)
            {
                template = await _repository.GetDefaultAsync(cancellationToken);
            }
        }

        template ??= GetFallbackDefaultTemplate();

        var engine = !string.IsNullOrWhiteSpace(template.EngineType) ? template.EngineType.Trim().ToUpperInvariant() : "EXCEL_LIBREOFFICE";
        if (engine == "EXCEL_LIBREOFFICE")
        {
            try
            {
                var excelBytes = await GenerateExcelStatementAsync(statement, template, cancellationToken);
                return await ConvertExcelToPdfViaLibreOfficeAsync(excelBytes, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Chuyển đổi Excel sang PDF qua LibreOffice không khả dụng ({Message}). Tự động fallback sang QuestPDF engine.", ex.Message);
            }
        }

        // QuestPDF rendering fallback
        var stream = new MemoryStream();
        BuildQuestPdfDocument(statement, template).GeneratePdf(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> GeneratePreviewPdfAsync(PreviewStatementPdfTemplateRequest templateRequest, CancellationToken cancellationToken = default)
    {
        var template = new StatementPdfTemplateEntity
        {
            HeaderTitle = templateRequest.HeaderTitle,
            SubHeaderTitle = templateRequest.SubHeaderTitle,
            CompanyName = templateRequest.CompanyName,
            CompanyAddress = templateRequest.CompanyAddress,
            ContactPhone = templateRequest.ContactPhone,
            LogoBase64 = templateRequest.LogoBase64,
            PaperSize = templateRequest.PaperSize,
            EngineType = templateRequest.EngineType,
            ExcelTemplateBase64 = templateRequest.ExcelTemplateBase64,
            ExcelTemplateFileName = templateRequest.ExcelTemplateFileName,
            DisplayColumnsJson = templateRequest.DisplayColumnsJson,
            FooterNotes = templateRequest.FooterNotes,
            RequireDualSignatures = templateRequest.RequireDualSignatures
        };

        var mockStatement = CreateMockStatement();

        var engine = !string.IsNullOrWhiteSpace(template.EngineType) ? template.EngineType.Trim().ToUpperInvariant() : "EXCEL_LIBREOFFICE";
        if (engine == "EXCEL_LIBREOFFICE")
        {
            try
            {
                var excelBytes = await GenerateExcelStatementAsync(mockStatement, template, cancellationToken);
                return await ConvertExcelToPdfViaLibreOfficeAsync(excelBytes, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Xem trước PDF qua LibreOffice không khả dụng ({Message}). Tự động fallback sang QuestPDF engine.", ex.Message);
            }
        }

        var stream = new MemoryStream();
        BuildQuestPdfDocument(mockStatement, template).GeneratePdf(stream);
        return stream.ToArray();
    }

    public byte[] GetStandardExcelTemplate(string paperSize = "A5_LANDSCAPE")
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("MeterReading");

        var isLandscape = string.Equals(paperSize, "A5_LANDSCAPE", StringComparison.OrdinalIgnoreCase);

        // Page setup
        ws.PrinterSettings.Orientation = isLandscape ? eOrientation.Landscape : eOrientation.Portrait;
        ws.PrinterSettings.PaperSize = isLandscape ? ePaperSize.A5 : ePaperSize.A4;
        ws.PrinterSettings.FitToPage = true;
        ws.PrinterSettings.FitToWidth = 1;
        ws.PrinterSettings.FitToHeight = 1;
        ws.PrinterSettings.LeftMargin = 0.3M;
        ws.PrinterSettings.RightMargin = 0.3M;
        ws.PrinterSettings.TopMargin = 0.3M;
        ws.PrinterSettings.BottomMargin = 0.3M;

        ws.Cells.Style.Font.Name = "Arial";
        ws.Cells.Style.Font.Size = 9;

        // 1. Company Header
                // Embed VSIP Logo (Top-Left) and Decorative Stripes (Top-Right)
        try
        {
            var logoBytes = Convert.FromBase64String(DefaultLogoBase64);
            var logoPic = ws.Drawings.AddPicture("VsipLogo", new MemoryStream(logoBytes));
            logoPic.SetPosition(0, 4, 0, 8);
            logoPic.SetSize(120, 44);
        }
        catch { }

        try
        {
            var decorBytes = GetTopRightDecorBytes();
            var decorPic = ws.Drawings.AddPicture("TopRightDecor", new MemoryStream(decorBytes));
            decorPic.SetPosition(0, 0, 7, 10);
            decorPic.SetSize(110, 77);
        }
        catch { }

        ws.Cells["A1:H1"].Merge = true;
        ws.Cells["A1"].Value = "{{CompanyName}}";
        ws.Cells["A1"].Style.Font.Size = 13;
        ws.Cells["A1"].Style.Font.Bold = true;
        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        ws.Cells["A2:H2"].Merge = true;
        ws.Cells["A2"].Value = "{{CompanyAddress}}";
        ws.Cells["A2"].Style.Font.Size = 8;
        ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        ws.Cells["A3:H3"].Merge = true;
        ws.Cells["A3"].Value = "{{ContactPhone}}";
        ws.Cells["A3"].Style.Font.Size = 8;
        ws.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        // 2. Titles
        ws.Cells["A5:H5"].Merge = true;
        ws.Cells["A5"].Value = "{{HeaderTitle}}";
        ws.Cells["A5"].Style.Font.Size = 12;
        ws.Cells["A5"].Style.Font.Bold = true;
        ws.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        ws.Cells["A6:H6"].Merge = true;
        ws.Cells["A6"].Value = "{{SubHeaderTitle}}";
        ws.Cells["A6"].Style.Font.Size = 11;
        ws.Cells["A6"].Style.Font.Bold = true;
        ws.Cells["A6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        ws.Cells["A7:H7"].Merge = true;
        ws.Cells["A7"].Value = "(M&E Copy – Liên cho Phòng Cơ Điện)";
        ws.Cells["A7"].Style.Font.Size = 8.5f;
        ws.Cells["A7"].Style.Font.Italic = true;
        ws.Cells["A7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        // 3. Customer & Meter Metadata
        ws.Cells["A9:D9"].Merge = true;
        ws.Cells["A9"].Value = "Khách hàng (Customer): {{CustomerName}}";
        ws.Cells["A9"].Style.Font.Bold = true;

        ws.Cells["E9:H9"].Merge = true;
        ws.Cells["E9"].Value = "Kỳ chốt (Period): {{Period}}";
        ws.Cells["E9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

        ws.Cells["A10:D10"].Merge = true;
        ws.Cells["A10"].Value = "Địa chỉ (Address): {{Address}}";

        ws.Cells["E10:H10"].Merge = true;
        ws.Cells["E10"].Value = "Hệ số nhân (Multiplier): {{Multiplier}}";
        ws.Cells["E10"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

        ws.Cells["A11:D11"].Merge = true;
        ws.Cells["A11"].Value = "Mã điểm đo (Point Code): {{PointCode}}";
        ws.Cells["A11"].Style.Font.Bold = true;

        ws.Cells["E11:H11"].Merge = true;
        ws.Cells["E11"].Value = "Số chế tạo (Meter Serial): {{MeterSerial}}";
        ws.Cells["E11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        ws.Cells["E11"].Style.Font.Bold = true;

        // 4. Data Table Header
        int tableStartRow = 13;
        ws.Cells[tableStartRow, 1].Value = "Biểu giá điện\n(Tariff)";
        ws.Cells[tableStartRow, 2].Value = "Chỉ số cũ\n(Previous)";
        ws.Cells[tableStartRow, 3].Value = "Chỉ số mới\n(Current)";
        ws.Cells[tableStartRow, 4].Value = "Độ lệch\n(Difference)";
        ws.Cells[tableStartRow, 5].Value = "HSN\n(Factor)";
        ws.Cells[tableStartRow, 6].Value = "Sản lượng (kWh)\n(Consumption)";
        ws.Cells[tableStartRow, 7].Value = "Pmax\n(kW)";
        ws.Cells[tableStartRow, 8].Value = "Cos φ";

        using (var headerRange = ws.Cells[tableStartRow, 1, tableStartRow, 8])
        {
            headerRange.Style.Font.Bold = true;
            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            headerRange.Style.WrapText = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240, 244, 248));
            headerRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            headerRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            headerRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }
        ws.Row(tableStartRow).Height = 28;

        // Data Rows
        var rowsData = new (string Label, string Prev, string Curr, string Diff, string Hsn, string Cons, string Pmax, string CosPhi, bool IsBold)[]
        {
            ("1. Bình thường (Normal - BT)", "{{PreviousBt}}", "{{Bt}}", "{{DiffBt}}", "{{Multiplier}}", "{{ConsumtionBt}}", "-", "-", false),
            ("2. Cao điểm (Peak - CD)", "{{PreviousCd}}", "{{Cd}}", "{{DiffCd}}", "{{Multiplier}}", "{{ConsumtionCd}}", "-", "-", false),
            ("3. Thấp điểm (Off-Peak - TD)", "{{PreviousTd}}", "{{Td}}", "{{DiffTd}}", "{{Multiplier}}", "{{ConsumtionTd}}", "-", "-", false),
            ("Tổng điện năng hữu công (Total kWh)", "{{PreviousSg}}", "{{Sg}}", "{{DiffSg}}", "{{Multiplier}}", "{{ConsumtionSg}}", "{{Pmax}}", "{{CosPhi}}", true),
            ("Vô công (Reactive - kVARh)", "{{PreviousVc}}", "{{Vc}}", "{{DiffVc}}", "{{Multiplier}}", "{{ConsumtionVc}}", "-", "-", false)
        };

        int currentRow = tableStartRow + 1;
        foreach (var r in rowsData)
        {
            ws.Cells[currentRow, 1].Value = r.Label;
            ws.Cells[currentRow, 2].Value = r.Prev;
            ws.Cells[currentRow, 3].Value = r.Curr;
            ws.Cells[currentRow, 4].Value = r.Diff;
            ws.Cells[currentRow, 5].Value = r.Hsn;
            ws.Cells[currentRow, 6].Value = r.Cons;
            ws.Cells[currentRow, 7].Value = r.Pmax;
            ws.Cells[currentRow, 8].Value = r.CosPhi;

            if (r.IsBold)
            {
                ws.Cells[currentRow, 1, currentRow, 8].Style.Font.Bold = true;
                ws.Cells[currentRow, 1, currentRow, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[currentRow, 1, currentRow, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(248, 249, 250));
            }

            ws.Cells[currentRow, 2, currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[currentRow, 6, currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[currentRow, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            for (int c = 1; c <= 8; c++)
            {
                ws.Cells[currentRow, c].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[currentRow, c].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[currentRow, c].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[currentRow, c].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }
            ws.Row(currentRow).Height = 20;
            currentRow++;
        }

        // 5. Pmax note and footer
        currentRow++;
        ws.Cells[currentRow, 1, currentRow, 8].Merge = true;
        ws.Cells[currentRow, 1].Value = "* Công suất cực đại Pmax: {{Pmax}} kW (Ghi nhận lúc: {{PmaxTime}})";
        ws.Cells[currentRow, 1].Style.Font.Bold = true;
        currentRow++;

        ws.Cells[currentRow, 1, currentRow, 8].Merge = true;
        ws.Cells[currentRow, 1].Value = "Ghi chú: {{FooterNotes}}";
        ws.Cells[currentRow, 1].Style.Font.Italic = true;
        ws.Cells[currentRow, 1].Style.Font.Size = 8;
        currentRow += 2;

        // 6. Dual Signatures
        ws.Cells[currentRow, 1, currentRow, 4].Merge = true;
        ws.Cells[currentRow, 1].Value = "Party A (Supplier) / Bên A (Đơn vị bán điện - VSIP Hải Phòng)";
        ws.Cells[currentRow, 1].Style.Font.Bold = true;
        ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        ws.Cells[currentRow, 5, currentRow, 8].Merge = true;
        ws.Cells[currentRow, 5].Value = "Party B (Tenant) / Bên B (Khách hàng mua điện)";
        ws.Cells[currentRow, 5].Style.Font.Bold = true;
        ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        currentRow++;
        ws.Cells[currentRow, 1, currentRow, 4].Merge = true;
        ws.Cells[currentRow, 1].Value = "Name and Signature / (Ký và ghi rõ họ tên)";
        ws.Cells[currentRow, 1].Style.Font.Size = 8;
        ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        ws.Cells[currentRow, 5, currentRow, 8].Merge = true;
        ws.Cells[currentRow, 5].Value = "Name and Signature / (Ký và ghi rõ họ tên)";
        ws.Cells[currentRow, 5].Style.Font.Size = 8;
        ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        // Column widths
        ws.Column(1).Width = 32;
        ws.Column(2).Width = 14;
        ws.Column(3).Width = 14;
        ws.Column(4).Width = 14;
        ws.Column(5).Width = 10;
        ws.Column(6).Width = 18;
        ws.Column(7).Width = 14;
        ws.Column(8).Width = 10;

        return package.GetAsByteArray();
    }

    public Task<byte[]> GenerateExcelStatementAsync(PeriodicEnergyStatementEntity statement, StatementPdfTemplateEntity? template = null, CancellationToken cancellationToken = default)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        template ??= GetFallbackDefaultTemplate();

        ExcelPackage package;
        if (!string.IsNullOrWhiteSpace(template.ExcelTemplateBase64))
        {
            try
            {
                var cleanBase64 = template.ExcelTemplateBase64.Contains(",")
                    ? template.ExcelTemplateBase64.Substring(template.ExcelTemplateBase64.IndexOf(",") + 1)
                    : template.ExcelTemplateBase64;
                var bytes = Convert.FromBase64String(cleanBase64.Trim());
                package = new ExcelPackage(new MemoryStream(bytes));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể đọc file template Excel tải lên từ Base64. Sử dụng mẫu Excel chuẩn: {Message}", ex.Message);
                package = new ExcelPackage(new MemoryStream(GetStandardExcelTemplate(template.PaperSize)));
            }
        }
        else
        {
            package = new ExcelPackage(new MemoryStream(GetStandardExcelTemplate(template.PaperSize)));
        }

        using (package)
        {
                        var ws = package.Workbook.Worksheets.FirstOrDefault() ?? package.Workbook.Worksheets.Add("MeterReading");

            // Ensure VSIP Logo and Top-Right Decorative Emblem are embedded in the worksheet
            if (ws.Drawings["VsipLogo"] == null)
            {
                try
                {
                    var logoBase64 = !string.IsNullOrWhiteSpace(template.LogoBase64) ? template.LogoBase64 : DefaultLogoBase64;
                    if (logoBase64.Contains(",")) logoBase64 = logoBase64.Substring(logoBase64.IndexOf(",") + 1);
                    var logoBytes = Convert.FromBase64String(logoBase64.Trim());
                    var logoPic = ws.Drawings.AddPicture("VsipLogo", new MemoryStream(logoBytes));
                    logoPic.SetPosition(0, 4, 0, 8);
                    logoPic.SetSize(120, 44);
                }
                catch { }
            }

            if (ws.Drawings["TopRightDecor"] == null)
            {
                try
                {
                    var decorBytes = GetTopRightDecorBytes();
                    var decorPic = ws.Drawings.AddPicture("TopRightDecor", new MemoryStream(decorBytes));
                    decorPic.SetPosition(0, 0, 7, 10);
                    decorPic.SetSize(110, 77);
                }
                catch { }
            }

            var customerName = statement.MeasurementPoint?.Customer?.FullName ?? "---";
            var customerCode = statement.MeasurementPoint?.Customer?.CustomerCode ?? "---";
            var address = statement.MeasurementPoint?.Address ?? statement.MeasurementPoint?.Customer?.Address ?? "---";
            var pointName = statement.MeasurementPoint?.Description ?? statement.MeasurementPoint?.Code ?? "---";
            var pointCode = statement.MeasurementPoint?.Code ?? "---";
            var serialNumber = !string.IsNullOrWhiteSpace(statement.SerialNumber) ? statement.SerialNumber : "---";
            var mult = statement.Multiplier > 0 ? statement.Multiplier : (statement.MeasurementPoint?.MultiplierFactor ?? 1);

            var fromDateStr = statement.PreviousDay.HasValue ? $"{statement.PreviousDay:00}/{statement.PreviousMonth:00}/{statement.PreviousYear}" : "01/10/2026";
            var toDateStr = $"{statement.Day:00}/{statement.Month:00}/{statement.Year}";
            var periodStr = $"{fromDateStr} - {toDateStr}";

            var diffBt = statement.Bt - statement.PreviousBt;
            var diffCd = statement.Cd - statement.PreviousCd;
            var diffTd = statement.Td - statement.PreviousTd;
            var diffSg = statement.Sg - statement.PreviousSg;
            var diffVc = statement.Vc - statement.PreviousVc;

            var pmaxStr = statement.Pmax > 0 ? statement.Pmax.ToString("N2") : "-";
            var pmaxTimeStr = statement.PmaxTime.HasValue ? statement.PmaxTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "---";
            var billingTimeStr = statement.BillingTimestamp.HasValue ? statement.BillingTimestamp.Value.ToString("dd/MM/yyyy HH:mm:ss") : toDateStr;

            // Cos phi calculation (ưu tiên giá trị đã lưu, hoặc tự động tính từ AP và AQ)
            var cosPhiStr = "-";
            if (statement.CosPhi.HasValue && statement.CosPhi.Value > 0)
            {
                cosPhiStr = statement.CosPhi.Value.ToString("F2");
            }
            else if (statement.ConsumtionSg > 0 && statement.ConsumtionVc >= 0)
            {
                var p = statement.ConsumtionSg;
                var q = statement.ConsumtionVc;
                var s = Math.Sqrt((p * p) + (q * q));
                if (s > 0)
                {
                    cosPhiStr = (p / s).ToString("F2");
                }
            }

            var replacements = new Dictionary<string, object>
            {
                { "{{CompanyName}}", template.CompanyName ?? "VSIP HAI PHONG CO.,LTD" },
                { "{{CompanyAddress}}", template.CompanyAddress ?? "" },
                { "{{ContactPhone}}", template.ContactPhone ?? "" },
                { "{{HeaderTitle}}", template.HeaderTitle ?? "MONTHLY POWER METER READING" },
                { "{{SubHeaderTitle}}", template.SubHeaderTitle ?? "(PHIẾU GHI SỐ ĐIỆN HÀNG THÁNG)" },
                { "{{CustomerName}}", customerName },
                { "{{CustomerCode}}", customerCode },
                { "{{Address}}", address },
                { "{{PointName}}", pointName },
                { "{{PointCode}}", pointCode },
                { "{{MeterSerial}}", serialNumber },
                { "{{Period}}", periodStr },
                { "{{BillingTimestamp}}", billingTimeStr },
                { "{{Multiplier}}", mult },
                { "{{PreviousBt}}", statement.PreviousBt },
                { "{{Bt}}", statement.Bt },
                { "{{DiffBt}}", diffBt },
                { "{{ConsumtionBt}}", statement.ConsumtionBt },
                { "{{PreviousCd}}", statement.PreviousCd },
                { "{{Cd}}", statement.Cd },
                { "{{DiffCd}}", diffCd },
                { "{{ConsumtionCd}}", statement.ConsumtionCd },
                { "{{PreviousTd}}", statement.PreviousTd },
                { "{{Td}}", statement.Td },
                { "{{DiffTd}}", diffTd },
                { "{{ConsumtionTd}}", statement.ConsumtionTd },
                { "{{PreviousSg}}", statement.PreviousSg },
                { "{{Sg}}", statement.Sg },
                { "{{DiffSg}}", diffSg },
                { "{{ConsumtionSg}}", statement.ConsumtionSg },
                { "{{PreviousVc}}", statement.PreviousVc },
                { "{{Vc}}", statement.Vc },
                { "{{DiffVc}}", diffVc },
                { "{{ConsumtionVc}}", statement.ConsumtionVc },
                { "{{Pmax}}", pmaxStr },
                { "{{PmaxTime}}", pmaxTimeStr },
                { "{{CosPhi}}", cosPhiStr },
                { "{{FooterNotes}}", template.FooterNotes ?? "" }
            };

            // Scan all cells and perform substitution
            var start = ws.Dimension?.Start;
            var end = ws.Dimension?.End;
            if (start != null && end != null)
            {
                for (int row = start.Row; row <= end.Row; row++)
                {
                    for (int col = start.Column; col <= end.Column; col++)
                    {
                        var cell = ws.Cells[row, col];
                        if (cell.Value == null) continue;

                        var text = cell.Value.ToString() ?? string.Empty;
                        if (!text.Contains("{{")) continue;

                        // Check if the entire cell is an exact token
                        var trimmedText = text.Trim();
                        if (replacements.TryGetValue(trimmedText, out var exactValue))
                        {
                            if (exactValue is double dVal)
                            {
                                cell.Value = dVal;
                                cell.Style.Numberformat.Format = "#,##0.00";
                            }
                            else if (exactValue is int iVal)
                            {
                                cell.Value = iVal;
                                cell.Style.Numberformat.Format = "#,##0";
                            }
                            else
                            {
                                cell.Value = exactValue.ToString();
                            }
                            continue;
                        }

                        // Otherwise string replace
                        foreach (var kvp in replacements)
                        {
                            if (text.Contains(kvp.Key))
                            {
                                text = text.Replace(kvp.Key, kvp.Value is double d ? d.ToString("N2") : kvp.Value.ToString());
                            }
                        }
                        cell.Value = text;
                    }
                }
            }

            return Task.FromResult(package.GetAsByteArray());
        }
    }

    public async Task<byte[]> ConvertExcelToPdfViaLibreOfficeAsync(byte[] excelBytes, CancellationToken cancellationToken = default)
    {
        var sofficePath = FindLibreOfficeExecutable();
        if (string.IsNullOrWhiteSpace(sofficePath) || !File.Exists(sofficePath))
        {
            throw new FileNotFoundException("Không tìm thấy trình chuyển đổi LibreOffice (soffice.exe). Vui lòng cấu hình 'LibreOfficePath' trong appsettings.json hoặc cài đặt LibreOffice trên máy chủ.");
        }

        var tempDir = Path.Combine(Path.GetTempPath(), "MDMS_ExcelPdf_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var excelFile = Path.Combine(tempDir, "statement.xlsx");
        var expectedPdfFile = Path.Combine(tempDir, "statement.pdf");

        try
        {
            await File.WriteAllBytesAsync(excelFile, excelBytes, cancellationToken);

            var psi = new ProcessStartInfo
            {
                FileName = sofficePath,
                Arguments = $"--headless --convert-to pdf --outdir \"{tempDir}\" \"{excelFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            var processWaitTask = process.WaitForExitAsync(cancellationToken);

            var completedTask = await Task.WhenAny(processWaitTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                try { process.Kill(true); } catch { }
                throw new TimeoutException("Quá trình chuyển đổi Excel sang PDF qua LibreOffice vượt quá thời gian chờ (30 giây).");
            }

            if (!File.Exists(expectedPdfFile))
            {
                var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
                throw new InvalidOperationException($"Không tìm thấy file PDF sau khi chuyển đổi. Mã lỗi: {process.ExitCode}. Output: {stdout}. Error: {stderr}");
            }

            return await File.ReadAllBytesAsync(expectedPdfFile, cancellationToken);
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể dọn dẹp thư mục tạm {TempDir}: {Message}", tempDir, ex.Message);
            }
        }
    }

    private string? FindLibreOfficeExecutable()
    {
        var configured = _configuration["LibreOfficePath"];
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
        {
            return configured;
        }

        var commonPaths = new[]
        {
            @"C:\Program Files\LibreOffice\program\soffice.exe",
            @"C:\Program Files (x86)\LibreOffice\program\soffice.exe",
            "/usr/bin/soffice",
            "/usr/local/bin/soffice"
        };

        foreach (var path in commonPaths)
        {
            if (File.Exists(path)) return path;
        }

        return null;
    }

    private static IDocument BuildQuestPdfDocument(PeriodicEnergyStatementEntity statement, StatementPdfTemplateEntity template)
    {
        var isLandscape = string.Equals(template.PaperSize, "A5_LANDSCAPE", StringComparison.OrdinalIgnoreCase);
        var pageSize = isLandscape ? new PageSize(595f, 420f) : PageSizes.A4;

        byte[] logoData;
        try
        {
            var logoStr = !string.IsNullOrWhiteSpace(template.LogoBase64) ? template.LogoBase64 : DefaultLogoBase64;
            if (logoStr.Contains(","))
            {
                logoStr = logoStr.Substring(logoStr.IndexOf(",") + 1);
            }
            logoData = Convert.FromBase64String(logoStr.Trim());
        }
        catch
        {
            try { logoData = Convert.FromBase64String(DefaultLogoBase64); } catch { logoData = Array.Empty<byte>(); }
        }

        byte[] decorData;
        try { decorData = GetTopRightDecorBytes(); } catch { decorData = Array.Empty<byte>(); }

        var columns = ParseColumns(template.DisplayColumnsJson);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(pageSize);
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(isLandscape ? 8 : 9));

                page.Content().Column(col =>
                {
                    // 1. Header with VSIP Logo (Left) and 3-Stripe Decorative Emblem (Right)
                    col.Item().PaddingTop(isLandscape ? 8 : 14).PaddingBottom(5).Row(headerRow =>
                    {
                        if (logoData != null && logoData.Length > 0)
                        {
                            headerRow.ConstantColumn(100).AlignLeft().Image(logoData).FitArea();
                        }
                        else
                        {
                            headerRow.ConstantColumn(100).AlignLeft();
                        }

                        headerRow.RelativeColumn().PaddingHorizontal(8).Column(center =>
                        {
                            center.Item().Text(template.CompanyName).FontSize(isLandscape ? 12 : 14).AlignCenter().Bold();
                            center.Item().Text(template.CompanyAddress).FontSize(isLandscape ? 7f : 8f).AlignCenter();
                            center.Item().Text(template.ContactPhone).FontSize(isLandscape ? 7f : 8f).AlignCenter();
                        });

                        if (decorData != null && decorData.Length > 0)
                        {
                            headerRow.ConstantColumn(95).AlignRight().Image(decorData).FitArea();
                        }
                        else
                        {
                            headerRow.ConstantColumn(95).AlignRight();
                        }
                    });

                    // 2. Title Section
                    col.Item().PaddingHorizontal(10).PaddingVertical(isLandscape ? 4 : 8).Column(titleCol =>
                    {
                        titleCol.Item().Text(template.HeaderTitle).FontSize(isLandscape ? 11 : 13).Bold().AlignCenter();
                        titleCol.Item().Text(template.SubHeaderTitle).FontSize(isLandscape ? 11 : 13).Bold().AlignCenter();
                        titleCol.Item().Text("(M&E Copy – Liên cho Phòng Cơ Điện)").FontSize(isLandscape ? 9 : 10).AlignCenter();
                    });

                    // 3. Customer & Measurement Point Info
                    col.Item().PaddingHorizontal(10).PaddingBottom(isLandscape ? 6 : 10).Row(infoRow =>
                    {
                        var customerName = statement.MeasurementPoint?.Customer?.FullName ?? "---";
                        var pointCode = statement.MeasurementPoint?.Code ?? "---";
                        var address = statement.MeasurementPoint?.Address ?? statement.MeasurementPoint?.Customer?.Address ?? "---";
                        var serialNumber = statement.SerialNumber;

                        infoRow.RelativeColumn().Column(left =>
                        {
                            left.Item().Text($"Customer (Khách hàng): {customerName}").Bold();
                            left.Item().Text($"Address (Địa chỉ): {address}");
                            left.Item().Text($"Point Code (Mã điểm đo): {pointCode}     Serial No: {serialNumber}").Bold();
                        });

                        var fromDateStr = statement.PreviousDay.HasValue ? $"{statement.PreviousDay:00}/{statement.PreviousMonth:00}/{statement.PreviousYear}" : "01/10/2026";
                        var toDateStr = $"{statement.Day:00}/{statement.Month:00}/{statement.Year}";

                        infoRow.ConstantColumn(200).AlignRight().Column(right =>
                        {
                            right.Item().Text($"Period (Kỳ): {fromDateStr} - {toDateStr}");
                            right.Item().Text($"Meter Multiplier (Hệ số nhân): {statement.Multiplier}");
                        });
                    });

                    // 4. Data Table
                    col.Item().PaddingHorizontal(10).Table(table =>
                    {
                        table.ColumnsDefinition(def =>
                        {
                            def.RelativeColumn(1.8f); // Tariff
                            def.RelativeColumn(1.2f); // Previous
                            def.RelativeColumn(1.2f); // Current
                            def.RelativeColumn(1.2f); // Difference
                            def.RelativeColumn(0.8f); // Multiplier
                            def.RelativeColumn(1.4f); // Consumption
                            if (columns.Contains("PMAX")) def.RelativeColumn(1.2f);
                            if (columns.Contains("COSPHI")) def.RelativeColumn(1.0f);
                        });

                        // Header Row
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).AlignCenter().Text("Biểu giá điện\n(Tariff)").Bold();
                            header.Cell().Element(CellStyle).AlignCenter().Text("Chỉ số cũ\n(Previous)").Bold();
                            header.Cell().Element(CellStyle).AlignCenter().Text("Chỉ số mới\n(Current)").Bold();
                            header.Cell().Element(CellStyle).AlignCenter().Text("Độ lệch\n(Difference)").Bold();
                            header.Cell().Element(CellStyle).AlignCenter().Text("HSN\n(Factor)").Bold();
                            header.Cell().Element(CellStyle).AlignCenter().Text("Sản lượng (kWh)\n(Consumption)").Bold();
                            if (columns.Contains("PMAX")) header.Cell().Element(CellStyle).AlignCenter().Text("Pmax\n(kW)").Bold();
                            if (columns.Contains("COSPHI")) header.Cell().Element(CellStyle).AlignCenter().Text("Cos φ").Bold();
                        });

                        var mult = statement.Multiplier > 0 ? statement.Multiplier : (statement.MeasurementPoint?.MultiplierFactor ?? 1);

                        void AddRow(string label, double prev, double curr, double diff, double cons, double? pmax = null, double? cosphi = null)
                        {
                            table.Cell().Element(CellStyle).Text(label);
                            table.Cell().Element(CellStyle).AlignRight().Text($"{prev:N2}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{curr:N2}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{diff:N2}");
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{mult}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{cons:N2}").Bold();
                            if (columns.Contains("PMAX")) table.Cell().Element(CellStyle).AlignRight().Text(pmax.HasValue && pmax.Value > 0 ? $"{pmax.Value:N2}" : "-");
                            if (columns.Contains("COSPHI")) table.Cell().Element(CellStyle).AlignCenter().Text(cosphi.HasValue && cosphi.Value > 0 ? $"{cosphi.Value:N2}" : "-");
                        }

                        double? effectiveCosPhi = statement.CosPhi;
                        if (!effectiveCosPhi.HasValue && statement.ConsumtionSg > 0 && statement.ConsumtionVc >= 0)
                        {
                            var sApparent = Math.Sqrt((statement.ConsumtionSg * statement.ConsumtionSg) + (statement.ConsumtionVc * statement.ConsumtionVc));
                            if (sApparent > 0)
                            {
                                effectiveCosPhi = Math.Round(statement.ConsumtionSg / sApparent, 2, MidpointRounding.AwayFromZero);
                            }
                        }

                        if (columns.Contains("BT")) AddRow("1. Bình thường (Normal - BT)", statement.PreviousBt, statement.Bt, statement.Bt - statement.PreviousBt, statement.ConsumtionBt);
                        if (columns.Contains("CD")) AddRow("2. Cao điểm (Peak - CD)", statement.PreviousCd, statement.Cd, statement.Cd - statement.PreviousCd, statement.ConsumtionCd);
                        if (columns.Contains("TD")) AddRow("3. Thấp điểm (Off-Peak - TD)", statement.PreviousTd, statement.Td, statement.Td - statement.PreviousTd, statement.ConsumtionTd);
                        if (columns.Contains("SG")) AddRow("Tổng điện năng hữu công (Total kWh)", statement.PreviousSg, statement.Sg, statement.Sg - statement.PreviousSg, statement.ConsumtionSg, statement.Pmax, effectiveCosPhi);
                        if (columns.Contains("VC")) AddRow("Vô công (Reactive - kVARh)", statement.PreviousVc, statement.Vc, statement.Vc - statement.PreviousVc, statement.ConsumtionVc);
                    });

                    // 5. Pmax Detail & Notes
                    col.Item().PaddingHorizontal(10).PaddingTop(isLandscape ? 4 : 8).Column(notesCol =>
                    {
                        if (statement.Pmax > 0)
                        {
                            var pmaxTimeStr = statement.PmaxTime.HasValue ? statement.PmaxTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "---";
                            notesCol.Item().Text($"* Công suất cực đại Pmax: {statement.Pmax:N2} kW (Ghi nhận lúc: {pmaxTimeStr})").Bold();
                        }

                        if (!string.IsNullOrWhiteSpace(template.FooterNotes))
                        {
                            notesCol.Item().PaddingTop(2).Text($"Ghi chú: {template.FooterNotes}").Italic().FontSize(isLandscape ? 7.5f : 8.5f);
                        }
                    });

                    // 6. Signatures
                    col.Item().PaddingHorizontal(10).PaddingTop(isLandscape ? 8 : 16).Row(sigRow =>
                    {
                        if (template.RequireDualSignatures)
                        {
                            sigRow.RelativeColumn(0.48f).Element(PartyBoxStyle).Column(partyA =>
                            {
                                partyA.Item().Text("Party A (Supplier)").Bold();
                                partyA.Item().Text("Bên A (Đơn vị bán điện - VSIP Hải Phòng)");
                                partyA.Item().PaddingVertical(isLandscape ? 8 : 15);
                                partyA.Item().Text("Name and Signature \n(Ký và ghi rõ họ tên)").FontSize(7.5f);
                            });

                            sigRow.RelativeColumn(0.04f);

                            sigRow.RelativeColumn(0.48f).Element(PartyBoxStyle).Column(partyB =>
                            {
                                partyB.Item().Text("Party B (Tenant)").Bold();
                                partyB.Item().Text("Bên B (Khách hàng mua điện)");
                                partyB.Item().PaddingVertical(isLandscape ? 8 : 15);
                                partyB.Item().Text("Name and Signature \n(Ký và ghi rõ họ tên)").FontSize(7.5f);
                            });
                        }
                        else
                        {
                            sigRow.RelativeColumn(0.5f).Element(PartyBoxStyle).Column(partyA =>
                            {
                                partyA.Item().Text("Party A (Supplier)").Bold();
                                partyA.Item().Text("Bên A (Đơn vị bán điện - VSIP Hải Phòng)");
                                partyA.Item().PaddingVertical(isLandscape ? 8 : 15);
                                partyA.Item().Text("Name and Signature \n(Ký và ghi rõ họ tên)").FontSize(7.5f);
                            });
                        }
                    });
                });
            });
        });
    }

    private static HashSet<string> ParseColumns(string? json)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "BT", "CD", "TD", "SG", "VC", "PMAX", "COSPHI" };
        if (string.IsNullOrWhiteSpace(json)) return set;

        try
        {
            var parsed = JsonSerializer.Deserialize<List<string>>(json);
            if (parsed != null && parsed.Count > 0)
            {
                return new HashSet<string>(parsed, StringComparer.OrdinalIgnoreCase);
            }
        }
        catch
        {
            // Fallback to full set
        }
        return set;
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.Border(0.5f).BorderColor(Colors.Black).Padding(3);
    }

    private static IContainer PartyBoxStyle(IContainer container)
    {
        return container.Border(0.5f).BorderColor(Colors.Black).Padding(4);
    }

    private static StatementPdfTemplateEntity GetFallbackDefaultTemplate()
    {
        return new StatementPdfTemplateEntity
        {
            Code = "DEFAULT_A5_VSIP",
            Name = "Phiếu Báo Điện Năng Chuẩn VSIP (A5 Ngang)",
            HeaderTitle = "MONTHLY POWER METER READING",
            SubHeaderTitle = "(PHIẾU GHI SỐ ĐIỆN HÀNG THÁNG)",
            CompanyName = "VSIP HAI PHONG CO.,LTD",
            CompanyAddress = "VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park, Hoa Binh Ward, Hai Phong City, Vietnam",
            ContactPhone = "Tel: 84-225-3959868   Fax: 84-255-3959886",
            PaperSize = "A5_LANDSCAPE",
            EngineType = "EXCEL_LIBREOFFICE",
            DisplayColumnsJson = "[\"BT\",\"CD\",\"TD\",\"SG\",\"VC\",\"PMAX\",\"COSPHI\"]",
            FooterNotes = "Quý khách vui lòng kiểm tra và phản hồi số liệu trong vòng 03 ngày làm việc kể từ ngày nhận phiếu báo.",
            RequireDualSignatures = true,
            IsDefault = true,
            IsActive = true
        };
    }

    private static PeriodicEnergyStatementEntity CreateMockStatement()
    {
        return new PeriodicEnergyStatementEntity
        {
            SerialNumber = "PB-202610-001",
            Multiplier = 120,
            MeasurementPoint = new MeasurementPointsEntity
            {
                Code = "101.402.001",
                Address = "Lô đất số IN1-4, KCN VSIP Hải Phòng, Xã An Lư, Huyện Thủy Nguyên, TP. Hải Phòng",
                MultiplierFactor = 120,
                Customer = new CustomerEntity
                {
                    FullName = "CÔNG TY TNHH KYOCERA VIỆT NAM",
                    Address = "Lô đất số IN1-4, KCN VSIP Hải Phòng"
                }
            },
            Day = 31,
            Month = 10,
            Year = 2026,
            PreviousDay = 1,
            PreviousMonth = 10,
            PreviousYear = 2026,
            PreviousBt = 124567.0,
            Bt = 125980.0,
            ConsumtionBt = 169560.0,
            PreviousCd = 34512.0,
            Cd = 34980.0,
            ConsumtionCd = 56160.0,
            PreviousTd = 56089.0,
            Td = 56890.0,
            ConsumtionTd = 96120.0,
            PreviousSg = 215168.0,
            Sg = 217850.0,
            ConsumtionSg = 321840.0,
            ConsumtionVc = 45200.0,
            Pmax = 1450.85,
            PmaxTime = new DateTime(2026, 10, 15, 14, 30, 0)
        };
    }
}
