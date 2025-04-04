using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using Microsoft.EntityFrameworkCore;
using TrackAndTrace_API.Models.DBModel;
using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using TrackAndTrace_API.Helpers;
using static TrackAndTrace_API.Helpers.Utils;

namespace TrackAndTrace_API.Repository
{
    public class IndentRepository : IIndentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public IndentRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(IndentDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var workFlowExists = await _context.Work_Flow.Where(x => x.code == model.indent_type).FirstOrDefaultAsync();
                if (workFlowExists == null)
                {
                    aPIResponseDTO.message = "Workflow does not exists";
                    return aPIResponseDTO;
                }

                var workFlowUserMappingExists = await _context.Work_Flow_Project_User_Mapping.Where(x => x.work_flow_id == workFlowExists.id && x.delete_flag == false).OrderBy(x => x.order_id).ToListAsync();
                if (workFlowUserMappingExists.Count == 0)
                {
                    aPIResponseDTO.message = $"{model.indent_type} - Workflow configuration not found";
                    return aPIResponseDTO;
                }

                string indentNo = string.Empty;

                if (model.indent_type == "MAI")
                {
                    var masterIndentExists = await _context.Indent.AnyAsync(x => x.project_id == model.project_id && x.indent_type == model.indent_type);
                    if (masterIndentExists)
                    {
                        aPIResponseDTO.message = "Master Indent already exists";
                        return aPIResponseDTO;
                    }

                    indentNo = "MAI_001";
                }
                else if (model.indent_type == "VAI" || model.indent_type == "NTI")
                {
                    var masterIndentExists = await _context.Indent.AnyAsync(x => x.project_id == model.project_id && x.indent_type == "MAI");
                    if (!masterIndentExists)
                    {
                        aPIResponseDTO.message = "Master Indent not found";
                        return aPIResponseDTO;
                    }

                    var variantIndentName = await _context.Indent.Where(x => x.project_id == model.project_id && x.indent_type == model.indent_type).OrderByDescending(x => x.id).Select(x => x.indent_no).FirstOrDefaultAsync();

                    indentNo = variantIndentName == null ? model.indent_type + "_001" : $"{model.indent_type}_{(int.Parse(variantIndentName.Split('_')[1]) + 1).ToString("D4")}";
                }

                var importName = await _context.Bulk_Import_Details.Where(x => x.company_id == token.CompanyId).OrderByDescending(x => x.id).Select(x => x.name).FirstOrDefaultAsync();

                var bulk_Import_Details = new Bulk_Import_Details
                {
                    company_id = token.CompanyId,
                    name = importName == null ? "IMPORT_0001" : $"IMPORT_{(int.Parse(importName.Split('_')[1]) + 1).ToString("D4")}",
                    status = "completed",
                    created_by = token.UserId,
                    created_date = DateTime.Now,
                    updated_date = null
                };

                await _context.Bulk_Import_Details.AddAsync(bulk_Import_Details);
                await _context.SaveChangesAsync();

                Indent indent = new Indent
                {
                    project_id = model.project_id,
                    request_id = bulk_Import_Details.id,
                    indent_date = model.indent_date,
                    indent_no = indentNo,
                    indent_type = model.indent_type,
                    status = "requested",
                    created_by = token.UserId,
                    created_date = DateTime.Now,
                    updated_date = null
                };

                await _context.Indent.AddAsync(indent);
                await _context.SaveChangesAsync();

                foreach (var item in model.indent_materials)
                {
                    Indent_Material indent_Material = new Indent_Material
                    {
                        indent_id = indent.id,
                        material_id = item.material_id,
                        quantity = item.quantity,
                        brand_id = item.brand_id,
                        lead_days = null,
                        delivery_date = null,
                        supply_cost = null,
                        remarks = null
                    };

                    await _context.Indent_Material.AddAsync(indent_Material);
                    await _context.SaveChangesAsync();

                    foreach (var differentiatorMappings in item.indent_material_differentiators)
                    {
                        differentiatorMappings.indent_material_id = indent_Material.id;
                    }

                    await _context.Indent_Material_Differentiator.AddRangeAsync(item.indent_material_differentiators);
                    await _context.SaveChangesAsync();
                }

                foreach (var item in workFlowUserMappingExists)
                {
                    Trx_Work_Flow_Approval_Status trx = new Trx_Work_Flow_Approval_Status
                    {
                        request_id = bulk_Import_Details.id,
                        wf_project_user_id = item.id,
                        user_id = item.user_id,
                        order_id = item.order_id,
                        is_supersede = item.is_supersede,
                        status = "pending",
                        created_by = token.UserId,
                        created_date = DateTime.Now,
                        updated_by = null,
                        updated_date = null
                    };

                    await _context.Trx_Work_Flow_Approval_Status.AddAsync(trx);
                    await _context.SaveChangesAsync();
                }

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Indent saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        public async Task<APIResponseDTO> GetList(int project_id, CommonRequestDto request, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                int totalCount = 0;
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_indent_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@project_id", project_id);
                        command.Parameters.AddWithValue("@page", request.page);
                        command.Parameters.AddWithValue("@page_size", request.page_size);
                        command.Parameters.AddWithValue("@search_query", request.search_query);
                        command.Parameters.AddWithValue("@sort_column", request.sort_column);
                        command.Parameters.AddWithValue("@sort_direction", request.sort_direction);

                        command.Parameters.Add("@total_count", SqlDbType.Int).Direction = ParameterDirection.Output;

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    request_id = reader.GetInt32(reader.GetOrdinal("request_id")),
                                    project_id = reader.GetInt32(reader.GetOrdinal("project_id")),
                                    project_name = reader.GetString(reader.GetOrdinal("project_name")),
                                    indent_type = reader.GetString(reader.GetOrdinal("indent_type")),
                                    indent_no = reader.GetString(reader.GetOrdinal("indent_no")),
                                    indent_date = reader.GetString(reader.GetOrdinal("indent_date")),
                                    indent_status = reader.GetString(reader.GetOrdinal("indent_status")),
                                    raised_by_id = reader.GetInt32(reader.GetOrdinal("raised_by_id")),
                                    raised_by_name = reader.GetString(reader.GetOrdinal("raised_by_name")),
                                    build_count = reader.GetInt32(reader.GetOrdinal("build_count")),
                                    unbuild_count = reader.GetInt32(reader.GetOrdinal("unbuild_count"))
                                };

                                list.Add(data);
                            }
                        }

                        // Get the total count from the output parameter
                        totalCount = (int)command.Parameters["@total_count"].Value;
                    }
                }

                response.success = true;
                response.message = list.Count > 0 ? "Data Fetched Successfully" : "No Records Found";
                response.data = list;
                response.total = totalCount;
                response.page = request.page;
                response.page_size = request.page_size;
            }
            catch (Exception ex)
            {
                response.success = false;
                response.message = ex.Message;
            }

            return response;
        }
        public async Task<APIResponseDTO> GetIndentDetails(int id, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var indentQuery = await (from a in _context.Bulk_Import_Details
                                         join b in _context.Indent on a.id equals b.request_id
                                         join c in _context.Project on b.project_id equals c.id
                                         join d in _context.Users on b.created_by equals d.id
                                         where a.id == id
                                         select new IndentByIdResponse
                                         {
                                             id = a.id,
                                             indent_type = b.indent_type,
                                             indent_no = b.indent_no,
                                             indent_date = b.indent_date,
                                             project_id = b.project_id,
                                             project_name = c.name,
                                             raised_by_id = token.UserId == b.created_by ? 0 : b.created_by,
                                             raised_by_name = d.name,
                                             approval_status = (from wf in _context.Trx_Work_Flow_Approval_Status
                                                                where wf.request_id == a.id
                                                                group wf by wf.request_id into g
                                                                select new
                                                                {
                                                                    status = g.All(w => w.status == "pending") ? "pending" :
                                                                             g.All(w => w.status == "approved") ? "approved" :
                                                                             g.Any(w => w.status == "rejected") ? "rejected" :
                                                                             g.Any(w => w.status == "cancelled") ? "cancelled" :
                                                                             "in-progress"
                                                                }).FirstOrDefault().status,
                                             indent_details = (from e in _context.Indent_Material
                                                               join f in _context.Material on e.material_id equals f.id
                                                               join g in _context.UOM on f.uom_id equals g.id
                                                               where e.indent_id == b.id && f.delete_flag == false && g.delete_flag == false
                                                               select new IndentDetails
                                                               {
                                                                   id = e.id,
                                                                   material_type = f.type,
                                                                   material_id = e.material_id,
                                                                   material_name = f.name,
                                                                   material_description = f.description,
                                                                   quantity = e.quantity,
                                                                   uom = g.name,
                                                                   lead_days = e.lead_days == null ? 0 : e.lead_days,
                                                                   delivery_date = e.delivery_date == null ? b.indent_date : e.delivery_date,
                                                                   supply_cost = e.supply_cost == null ? 0 : e.supply_cost,
                                                                   remarks = e.remarks
                                                               }).ToList()
                                         }).FirstOrDefaultAsync();

                // Ensure indentQuery is not null before proceeding
                if (indentQuery == null)
                {
                    response.success = false;
                    response.message = "No Records Found";
                    return response;
                }

                // Fetch material specifications separately
                var materialSpecifications = await (from h in _context.Indent_Material_Differentiator
                                                    join i in _context.Differentiator_Mapping on h.differentiator_mapping_id equals i.id
                                                    join j in _context.Differentiator on i.differentiator_id equals j.id
                                                    where indentQuery.indent_details.Select(x => x.id).Contains(h.indent_material_id)
                                                    select new
                                                    {
                                                        indent_material_id = h.indent_material_id,
                                                        name = j.name,
                                                        value = i.value
                                                    }).ToListAsync();

                // Group by indent_material_id and process concatenation in memory
                var groupedSpecifications = materialSpecifications
                    .GroupBy(x => x.indent_material_id)
                    .ToDictionary(
                        g => g.Key,
                        g => string.Join(", ", g.Select(x => x.name + ": " + x.value))
                    );

                // Merge specifications into indent details
                foreach (var detail in indentQuery.indent_details)
                {
                    detail.material_specification = groupedSpecifications.ContainsKey(detail.id)
                        ? groupedSpecifications[detail.id]
                        : "";
                }

                response.success = true;
                response.message = "Data Fetched Successfully";
                response.data = indentQuery;
            }
            catch (Exception ex)
            {
                response.success = false;
                response.message = ex.Message;
            }

            return response;
        }
        public async Task<APIResponseDTO> UpdateIndentRequestStatus(int request_id, string status, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();

            try
            {
                if (status != "cancelled")
                {
                    bool isFinalApproval = false;

                    var finalApprovalDetails = await _context.Trx_Work_Flow_Approval_Status.Where(x => x.request_id == request_id).OrderByDescending(x => x.order_id).FirstOrDefaultAsync();

                    if (finalApprovalDetails != null)
                    {
                        isFinalApproval = finalApprovalDetails.user_id == token.UserId ? true : false;
                    }

                    var requestDetails = await _context.Trx_Work_Flow_Approval_Status.Where(x => x.request_id == request_id && x.user_id == token.UserId).FirstOrDefaultAsync();

                    if (requestDetails != null)
                    {
                        requestDetails.status = status;
                        requestDetails.updated_by = token.UserId;
                        requestDetails.updated_date = DateTime.Now;

                        _context.Trx_Work_Flow_Approval_Status.Update(requestDetails);
                        await _context.SaveChangesAsync();

                        var indentRequestHdr = await _context.Indent.Where(x => x.request_id == request_id).FirstOrDefaultAsync();

                        if (indentRequestHdr != null)
                        {
                            indentRequestHdr.status = (isFinalApproval == false && status == "approved") ? "in-progress" : status;
                            indentRequestHdr.updated_by = token.UserId;
                            indentRequestHdr.updated_date = DateTime.Now;

                            _context.Indent.Update(indentRequestHdr);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    var boqRequestHdr = await _context.Indent.Where(x => x.request_id == request_id).FirstOrDefaultAsync();

                    if (boqRequestHdr != null)
                    {
                        var approvalStatus = await _context.Trx_Work_Flow_Approval_Status.Where(x => x.request_id == request_id).ToListAsync();

                        foreach (var approval in approvalStatus)
                        {
                            approval.status = "cancelled";
                            approval.updated_by = token.UserId;
                            approval.updated_date = DateTime.Now;
                        }

                        _context.Trx_Work_Flow_Approval_Status.UpdateRange(approvalStatus);
                        await _context.SaveChangesAsync();

                        boqRequestHdr.status = status;
                        boqRequestHdr.updated_by = token.UserId;
                        boqRequestHdr.updated_date = DateTime.Now;

                        _context.Indent.Update(boqRequestHdr);
                        await _context.SaveChangesAsync();
                    }
                }

                response.success = true;
                response.message = $"Request {status} Successfully";
            }
            catch (Exception ex)
            {
                response.success = false;
                response.message = ex.Message;
            }

            return response;
        }
        public async Task<APIResponseDTO> UpdateIndentMaterialDetails(UpdateIndentMaterialDto model)
        {
            APIResponseDTO response = new APIResponseDTO();

            try
            {
                if (model?.indent_materials == null || !model.indent_materials.Any())
                {
                    response.success = false;
                    response.message = "No indent materials provided.";
                    return response;
                }

                var ids = model.indent_materials.Select(x => x.id).ToList();
                var indentMaterials = await _context.Indent_Material.Where(x => ids.Contains(x.id)).ToListAsync();

                if (!indentMaterials.Any())
                {
                    response.success = false;
                    response.message = "No matching indent materials found.";
                    return response;
                }

                foreach (var indentMaterial in indentMaterials)
                {
                    var updateItem = model.indent_materials.FirstOrDefault(x => x.id == indentMaterial.id);
                    if (updateItem != null)
                    {
                        indentMaterial.lead_days = updateItem.lead_days;
                        indentMaterial.delivery_date = updateItem.delivery_date;
                        indentMaterial.supply_cost = updateItem.supply_cost;
                        indentMaterial.remarks = updateItem.remarks;
                    }
                }

                await _context.SaveChangesAsync();

                response.success = true;
                response.message = "Indent Material Details saved successfully.";
            }
            catch (Exception ex)
            {
                response.success = false;
                response.message = "An error occurred while updating indent materials.";
            }

            return response;
        }

    }
}